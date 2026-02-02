using LightORM.Extension;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
namespace LightORM;

internal static class ExpressionExtensions
{
    public static ExpressionResolvedResult Resolve(this Expression? expression, SqlResolveOptions options, ResolveContext context)
    {
        var resolve = new ExpressionResolver(options, context);
        resolve.Visit(expression);
        return new ExpressionResolvedResult
        {
            SqlString = resolve.Sql.ToString(),
            DbParameters = resolve.DbParameters,
            Members = resolve.ResolvedMembers,
            MemberOfNavigateMember = resolve.MemberOfNavigateMember,
            UseNavigate = resolve.UseNavigate,
            NavigateDeep = resolve.NavigateDeep,
            NavigateMembers = resolve.NavigateMembers,
            WindowFnPartials = resolve.WindowFnPartials,
            NavigateWhereExpression = resolve.NavigateWhereExpression
        };
    }

    public static string OperatorParser(this ExpressionType expressionNodeType, bool isNull)
    {
        return expressionNodeType switch
        {
            ExpressionType.And or
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Equal => isNull ? " IS " : " = ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.NotEqual => isNull ? " IS NOT " : " <> ",
            ExpressionType.Or or
            ExpressionType.OrElse => " OR ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.Add => " + ",
            ExpressionType.Subtract => " - ",
            ExpressionType.Multiply => " * ",
            ExpressionType.Divide => " / ",
            _ => throw new NotImplementedException("未实现的节点类型" + expressionNodeType)
        };
    }
}
internal class ExpressionResolver(SqlResolveOptions options, ResolveContext context) : IExpressionResolver
{
    public SqlResolveOptions Options { get; } = options;
    public ResolveContext Context { get; } = context;
    public List<DbParameterInfo> DbParameters { get; set; } = [];
    public StringBuilder Sql { get; set; } = new StringBuilder(128);
    public Stack<MemberInfo> Members { get; set; } = [];
    public List<string> ResolvedMembers { get; set; } = [];
    public List<WindowFnSpecification>? WindowFnPartials { get; set; }
    public bool IsNot { get; set; }
    public bool UseNavigate { get; set; }
    public int NavigateDeep { get; set; }
    public int Level => Context.Level;
    internal List<string> NavigateMembers { get; set; } = [];
    public Dictionary<string, Expression?>? ExpStores { get; set; }
    public Expression? NavigateWhereExpression { get; set; }
    public string? MemberOfNavigateMember { get; set; }
    private ISqlMethodResolver MethodResolver => Context.Database.MethodResolver;
    private ICustomDatabase Database => Context.Database;
    public Expression? Body => bodyExpression;
    public ReadOnlyCollection<ParameterExpression>? Parameters => parametersExpression;
    public Stack<ResolvePart> ResolveRecord { get; set; } = [];
    public Expression? Visit(Expression? expression)
    {
        //Debug.Write($"");
        return expression switch
        {
            LambdaExpression => Visit(VisitLambda((LambdaExpression)expression)),
            BinaryExpression => Visit(VisitBinary((BinaryExpression)expression)),
            ConditionalExpression => Visit(VisitConditional((ConditionalExpression)expression)),
            MethodCallExpression => Visit(VisitMethodCall((MethodCallExpression)expression)),
            NewArrayExpression => Visit(VisitNewArray((NewArrayExpression)expression)),
            NewExpression => Visit(VisitNew((NewExpression)expression)),
            UnaryExpression => Visit(VisitUnary((UnaryExpression)expression)),
            ParameterExpression => Visit(VisitParameter((ParameterExpression)expression)),
            MemberInitExpression => Visit(VisitMemberInit((MemberInitExpression)expression)),
            MemberExpression => Visit(VisitMember((MemberExpression)expression)),
            ConstantExpression => Visit(VisitConstant((ConstantExpression)expression)),
            _ => null
        };
    }

    private const string AS_LITERAL = " AS ";
    Expression? bodyExpression;
    ReadOnlyCollection<ParameterExpression>? parametersExpression;
    int parameterPositionIndex = 0;
    bool resolveNullValue;
    //string? lastResolvedColumnName;
    bool useAs = true;
    bool UseAs
    {
        get
        {
            if (!useAs)
            {
                useAs = true;
                return false;
            }
            return useAs && Options.UseColumnAlias;
        }
        set => useAs = value;
    }

    bool ResolveNullValue
    {
        get
        {
            if (resolveNullValue)
            {
                resolveNullValue = false;
                return true;
            }
            return resolveNullValue;
        }
        set => resolveNullValue = value;
    }

    bool isVisitConvert;

    Expression? VisitLambda(LambdaExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: LambdaExpression: {exp}");
        bodyExpression = exp.Body;
        parametersExpression = exp.Parameters;
        //Context.Tables.Clear();
        for (int i = 0; i < exp.Parameters.Count; i++)
        {
            ParameterExpression? item = exp.Parameters[i];
            Context.HandleParameterExpression(item, i);
        }
        return bodyExpression;
    }

    Expression? VisitBinary(BinaryExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: BinaryExpression: {exp}");

        // 数组访问
        if (exp.NodeType == ExpressionType.ArrayIndex)
        {
            var index = ExtractInstanceValue<int>(exp.Right);
            var array = ExtractInstanceValue<Array>(exp.Left);
            var arrayValue = array!.GetValue(index);
            var pname = FormatDbParameterName(Context, Options, $"Arr{index}", ref parameterPositionIndex);
            Sql.Append(pname);
            DbParameters.Add(new(pname, arrayValue, ExpValueType.Other));
            return null;
        }
        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join || Options.SqlType == SqlPartial.Select)
        {
            Sql.Append('(');
        }

        Visit(exp.Left);
        var insertIndex = Sql.Length;
        Visit(exp.Right);
        var op = exp.NodeType.OperatorParser(ResolveNullValue);
        Sql.Insert(insertIndex, op);

        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join || Options.SqlType == SqlPartial.Select)
        {
            Sql.Append(')');
        }

        return null;

        // 尝试将表达式求值为常量（仅支持 Constant 和 简单 Member 访问）
        static T ExtractInstanceValue<T>(Expression expression)
        {
            if (expression is ConstantExpression ce && ce.Value is T index)
            {
                return index;
            }
            var members = new Stack<MemberInfo>();
            Expression? current = expression;

            // 向下遍历，收集 MemberInfo
            while (current is MemberExpression memberExpr)
            {
                members.Push(memberExpr.Member);
                current = memberExpr.Expression;
            }
            object? value;

            if (current is ConstantExpression constExpr)
            {
                value = constExpr.Value;
            }
            else if (current is null)
            {
                value = null;
            }
            else
            {
                throw new LightOrmException($"数组索引表达式必须以常量或者Null结尾，但得到: {current?.GetType().Name}: {current}");
            }

            value = GetValue(members, value);

            if (value is T t)
            {
                return t;
            }
            throw new LightOrmException($"尝试获取类型{typeof(T)}的值，实际类型: {value?.GetType()}");
        }
    }

    Expression? VisitConditional(ConditionalExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ConditionalExpression: {exp}");
        Sql.Append("CASE WHEN ");
        Visit(exp.Test);
        Sql.Append(" THEN ");
        Visit(exp.IfTrue);
        Sql.Append(" ELSE ");
        Visit(exp.IfFalse);
        Sql.Append(" END");
        return null;
    }

    Expression? VisitMethodCall(MethodCallExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: MethodCallExpression: {exp}");
        ResolveRecord.Push(ResolvePart.MethodCall);
        Members.Clear();
        if (exp.Method.Name.Equals("get_Item") && (exp.Method.DeclaringType?.FullName?.StartsWith("System.Collections.Generic") ?? false))
        {

        }
        else
        {
            if (exp.Method.Name == "op_Implicit" && exp.Method.IsSpecialName)
            {
                return exp.Arguments[0];
            }
            MethodResolver.Resolve(this, exp);
        }
        return null;
    }

    Expression? VisitNewArray(NewArrayExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: NewArrayExpression: {exp}");
        for (int i = 0; i < exp.Expressions.Count; i++)
        {
            Visit(exp.Expressions[i]);
            if (i < exp.Expressions.Count - 1)
                Sql.Append(", ");
        }
        return null;
    }

    Expression? VisitNew(NewExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: NewExpression: {exp}");
        for (int i = 0; i < exp.Arguments.Count; i++)
        {
            var member = exp.Members![i];
            var arg = exp.Arguments[i];

            ResolvedMembers.Add(exp.Members[i].Name);
            if (Options.SqlType == SqlPartial.Select)
            {
                Visit(arg);
                if (UseAs)
                {
                    Sql.Append(AS_LITERAL);
                    Sql.AppendEmphasis(member.Name, Database);
                }
                //Sql.Append($" AS {Database.AttachEmphasis(member.Name)}");
            }
            else if (Options.SqlType == SqlPartial.Insert)
            {
                //var col = Context.GetTable(exp.Type).Columns.First(c => c.PropertyName == member.Name);
                //Sql.Append($"{Database.AttachEmphasis(col.ColumnName)} = ");
                Visit(arg);
            }
            else
            {
                Visit(arg);
            }


            if (i + 1 < exp.Arguments.Count)
            {
                Sql.Append(", ");
            }
        }
        return null;
    }

    Expression? VisitUnary(UnaryExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: UnaryExpression: {exp}");
        IsNot = exp.NodeType == ExpressionType.Not;
        isVisitConvert = exp.NodeType == ExpressionType.Convert;
        Visit(exp.Operand);
        return null;
    }

    Expression? VisitParameter(ParameterExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ParameterExpression: {exp}");
        if (Options.SqlType == SqlPartial.Select)
        {
            var alias = Context.GetTable(exp).Alias;
            //Sql.Append($"{alias}.*");
            Sql.Append(alias);
            Sql.Append(".*");
            UseAs = false;
        }
        else if (Options.SqlAction == SqlAction.Insert)
        {
            var table = Context.GetTable(exp);
            foreach (var item in table.TableEntityInfo.Columns)
            {
                var prop = Expression.Property(exp, item.PropertyName);
                Visit(prop);
                Sql.Append(", ");
            }
            Sql.RemoveLast(2);
        }
        else
        {
            if (isVisitConvert && Members.Count > 0)
            {
                isVisitConvert = false;
                var member = Members.Pop();
                var table = Context.GetTable(exp);
                var col = table.GetColumn(member.Name)!;
                if (Options.RequiredTableAlias)
                {
                    //Sql.Append($"{table.Alias}.{Database.AttachEmphasis(col.ColumnName)}");
                    Sql.Append(table.Alias).Append('.').AppendEmphasis(col.ColumnName, Database);
                }
                else
                {
                    //Sql.Append($"{Database.AttachEmphasis(col.ColumnName)}");
                    Sql.AppendEmphasis(col.ColumnName, Database);
                }
            }
        }

        return null;
    }

    Expression? VisitMemberInit(MemberInitExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: MemberInitExpression: {exp}");
        for (int i = 0; i < exp.Bindings.Count; i++)
        {
            if (exp.Bindings[i].BindingType != MemberBindingType.Assignment)
            {
                continue;
            }
            if (exp.Bindings[i] is not MemberAssignment memberAssign)
            {
                continue;
            }
            if (Options.SqlType == SqlPartial.Select)
            {
                Visit(memberAssign.Expression);

                if (UseAs)
                {
                    Sql.Append(AS_LITERAL);
                    Sql.AppendEmphasis(memberAssign.Member.Name, Database);
                }

                if (i + 1 < exp.Bindings.Count)
                {
                    Sql.Append(", ");
                }
            }
        }
        return null;
    }

    Expression? VisitMember(MemberExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: MemberExpression: {exp}");
        if (bodyExpression?.NodeType == ExpressionType.MemberAccess)
        {
            ResolvedMembers.Add(exp.Member.Name);
        }
        if (exp.Expression?.NodeType == ExpressionType.Parameter)
        {

            var paramExp = exp.Expression as ParameterExpression;
            //var pType = paramExp?.Type;

            var name = exp.Member.Name;
            var table = Context.GetTable(paramExp!);
            var col = table.GetColumn(name)!;
            if (col.IsNavigate == true)
            {
                UseNavigate = true;
                NavigateMembers.Add(col.PropertyName);
                if (NavigateDeep == 0)
                {
                    if (Members.Count > 0)
                    {
                        //ResolvedMembers.Add(Members.Pop().Name);
                        MemberOfNavigateMember = Members.Pop().Name;
                    }
                    Members.Clear();
                }
                //else
                //{
                //    return null;
                //}
                return null;
            }
            // TODO 表达式扁平化处理后，只有聚合属性才有 p.XXX.XXX ?
            if (Members.Count > 0)
            {
                if (col.IsAggregated)
                {
                    name = Members.Pop().Name;
                    col = table.GetColumnInfo(name);
                }
            }

            if (Options.SqlType == SqlPartial.Where)
            {
                ResolvedMembers.Add(col.PropertyName);
            }

            //if (!table.TableEntityInfo.IsAnonymousType)
            //{
            //    UseAs = col.ColumnName != col.PropertyName;
            //}
            if (Options.RequiredTableAlias)
            {
                Sql.Append(table.Alias);
                Sql.Append('.');
                //Sql.Append($"{table.Alias}.{Database.AttachEmphasis(col.ColumnName)}");
            }
            Sql.AppendEmphasis(col.ColumnName, Database);
            //lastResolvedColumnName = col.ColumnName;
            Members.Clear();
            return null;
        }
        Members.Push(exp.Member);
        return exp.Expression ?? Expression.Constant(exp.Type.TypeDefaultValue(), exp.Type);
    }

    Expression? VisitConstant(ConstantExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ConstantExpression: {exp}");
        var value = exp.Value;
        if (Members.Count > 0)
        {
            //value = GetValue(Members, value, out var name);
            //VariableValue(value, name);
            var v = GetValue(Members, value, out var propNames);
            var pn = FormatDbParameterName(Context, Options, propNames, ref parameterPositionIndex);
            Sql.Append(pn);
            VariableValue(v, pn);
        }
        else
        {
            ConstraintValue(value);
        }
        return null;

        void VariableValue(object? v, string bn)
        {
            if (v == null)
            {
                DbParameters.Add(new(bn, null, ExpValueType.Null));
            }
            else if (v is IEnumerable && v is not string)
            {
                DbParameters.Add(new(bn, v, ExpValueType.Collection));
            }
            else if (v is bool)
            {
                if (IsNot)
                {
                    DbParameters.Add(new(bn, v, ExpValueType.BooleanReverse));
                    IsNot = false;
                }
                else
                {
                    DbParameters.Add(new(bn, v, ExpValueType.Boolean));
                }
            }
            else
            {
                DbParameters.Add(new(bn, v, ExpValueType.Other));
            }
        }

        void ConstraintValue(object? v)
        {
            if (v == null)
            {
                Sql.Append("NULL");
                ResolveNullValue = true;
                return;
            }
            if (bodyExpression?.NodeType == ExpressionType.Constant && exp.Type == typeof(bool))
            {
                var b = (bool)v;
                Sql.Append("1 = ");
                Sql.Append(b ? "1" : "0");
            }
            else
            {
                if (exp.Type.IsNumber())
                {
                    Sql.Append(v.ToString());
                }
                else if (exp.Type.IsBoolean() && v is bool b)
                {
                    if (IsNot)
                    {
                        Sql.Append(Database.HandleBooleanValue(!b));
                        IsNot = false;
                    }
                    else
                    {
                        Sql.Append(Database.HandleBooleanValue(b));
                    }
                }
                else
                {
                    Sql.Append('\'');
                    Sql.Append(v.ToString());
                    Sql.Append('\'');
                }
            }
        }
    }

    public static string FormatDbParameterName(ResolveContext? context, SqlResolveOptions? option, string name, ref int index)
    {
        var p = $"{context?.ParameterPrefix}{name}_{option?.ParameterPartialIndex}_{index}";
        index += 1;
        return p;
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfos">成员信息</param>
    /// <param name="compilerVar">编译器变量值</param>
    /// <param name="memberName">成员名称</param>
    /// <returns></returns>
    public static object? GetValue(Stack<MemberInfo> memberInfos, object? compilerVar, out string memberName)
    {
        var names = new List<string>();
        while (memberInfos.Count > 0)
        {
            var item = memberInfos.Pop();
            if (!item.Name.StartsWith("CS$<>8__locals"))
            {
                names.Add(item.Name);
            }

            compilerVar = GetValue(item, compilerVar);
        }
        memberName = string.Join("_", names);
        return compilerVar;
    }

    public static object? GetValue(Stack<MemberInfo> memberInfos, object? compilerVar)
    {
        while (memberInfos.Count > 0)
        {
            var item = memberInfos.Pop();
            compilerVar = GetValue(item, compilerVar);
        }
        return compilerVar;
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfos">成员信息</param>
    /// <returns></returns>
    public static string GetValueName(Stack<MemberInfo> memberInfos)
    {
        var names = new List<string>();
        while (memberInfos.Count > 0)
        {
            var item = memberInfos.Pop();
            if (!item.Name.StartsWith("CS$<>8__locals"))
            {
                names.Add(item.Name);
            }

        }
        return string.Join("_", names);
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfo">成员信息</param>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? GetValue(MemberInfo memberInfo, object? obj)
    {
        return memberInfo switch
        {
            PropertyInfo prop => prop.GetValue(obj),
            FieldInfo field => field.GetValue(obj),
            _ => throw new NotSupportedException($"不支持获取 {memberInfo.MemberType} 类型值.")
        };
    }
}
