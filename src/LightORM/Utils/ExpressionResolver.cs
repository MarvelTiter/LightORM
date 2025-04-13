﻿using LightORM.Extension;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq.Expressions;
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
            //UsedTables = resolve.UsedTables,
            UseNavigate = resolve.UseNavigate,
            NavigateDeep = resolve.NavigateDeep,
            NavigateMembers = resolve.NavigateMembers,
            NavigateWhereExpression = resolve.NavigateWhereExpression
        };
    }

    public static string OperatorParser(this ExpressionType expressionNodeType, bool useIs)
    {
        return expressionNodeType switch
        {
            ExpressionType.And or
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Equal => useIs ? " IS " : " = ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.NotEqual => useIs ? " IS NOT " : " <> ",
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

public interface IExpressionResolver
{
    bool IsNot { get; }
    int NavigateDeep { get; set; }
    int Level { get; }
    Dictionary<string, Expression?>? ExpStores { get; set; }
    StringBuilder Sql { get; }
    Dictionary<string, object> DbParameters { get; }
    SqlResolveOptions Options { get; }
    Expression? NavigateWhereExpression { get; set; }
    Expression? Visit(Expression? expression);
}

internal class ExpressionResolver(SqlResolveOptions options, ResolveContext context) : IExpressionResolver
{
    public SqlResolveOptions Options { get; } = options;
    public ResolveContext Context { get; } = context;
    public Dictionary<string, object> DbParameters { get; set; } = [];
    public StringBuilder Sql { get; set; } = new StringBuilder();
    public Stack<MemberInfo> Members { get; set; } = [];
    public List<string> ResolvedMembers { get; set; } = [];
    //public List<TableInfo> UsedTables { get; set; } = [];
    public bool IsNot { get; set; }
    public bool UseNavigate { get; set; }
    public int NavigateDeep { get; set; }
    public int Level => Context.Level;
    internal List<string> NavigateMembers { get; set; } = [];
    public Dictionary<string, Expression?>? ExpStores { get; set; }
    public Expression? NavigateWhereExpression { get; set; }

    private ISqlMethodResolver MethodResolver => Context.Database.MethodResolver;
    private ICustomDatabase Database => Context.Database;
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
    Expression? bodyExpression;
    bool useAs;
    bool UseAs
    {
        get
        {
            if (useAs)
            {
                useAs = false;
                return true;
            }
            return useAs;
        }
        set => useAs = value;
    }


    bool isVisitConvert;
    Expression? VisitLambda(LambdaExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: LambdaExpression: {exp}");
        bodyExpression = exp.Body;
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
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: BinaryExpression: {exp}");
        // 数组访问
        if (exp.NodeType == ExpressionType.ArrayIndex)
        {
            var index = Convert.ToInt32(Expression.Lambda(exp.Right).Compile().DynamicInvoke());
            var array = Expression.Lambda(exp.Left).Compile().DynamicInvoke() as Array;
            var parameterName = AddDbParameter("Const", array!.GetValue(index)!);
            Sql.Append(parameterName);
            return null;
        }
        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join || Options.SqlType == SqlPartial.Select)
        {
            Sql.Append('(');
        }

        Visit(exp.Left);
        var insertIndex = Sql.Length;
        Visit(exp.Right);
        var endIndex = Sql.Length;
        var useIs = endIndex - insertIndex == 4 && Sql.ToString(insertIndex, 4) == "NULL";
        var op = exp.NodeType.OperatorParser(useIs);
        Sql.Insert(insertIndex, op);

        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join || Options.SqlType == SqlPartial.Select)
        {
            Sql.Append(')');
        }

        return null;
    }

    Expression? VisitConditional(ConditionalExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: ConditionalExpression: {exp}");
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
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: MethodCallExpression: {exp}");
        Members.Clear();
        if (exp.Method.Name.Equals("get_Item") && (exp.Method.DeclaringType?.FullName?.StartsWith("System.Collections.Generic") ?? false))
        {

        }
        else
        {
            //if (exp.Object is not null)
            //{
            //    var obj = Expression.Lambda(exp.Object).Compile().DynamicInvoke();
            //    if (obj is IExpSelect sel)
            //    {
            //        var sql = sel.SqlBuilder.ToSqlString();
            //    }
            //}
            if (Options.SqlType == SqlPartial.Select)
            {
                UseAs = true;
            }
            MethodResolver.Resolve(this, exp);
        }
        return null;
    }

    Expression? VisitNewArray(NewArrayExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: NewArrayExpression: {exp}");
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
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: NewExpression: {exp}");
        for (int i = 0; i < exp.Arguments.Count; i++)
        {
            var member = exp.Members![i];
            var arg = exp.Arguments[i];
            if (member.Name.StartsWith("Tb")
                && member is PropertyInfo prop
                && prop.DeclaringType?.FullName?.StartsWith("LightORM.TypeSet") == true)
            {
                //Debug.WriteLine("Visit TypeSet Property");
                //TODO 这段if分支，应该是不需要了
                Debug.Assert(false);
                ParameterExpression p = Expression.Parameter(prop.PropertyType, member.Name);
                Visit(p);
                UseAs = true;
            }
            else
            {
                if (Options.SqlType == SqlPartial.GroupBy)
                {
                    if (arg.NodeType == ExpressionType.MemberAccess && arg is MemberExpression e)
                    {
                        var parent = e.Expression;
                        if (parent is ParameterExpression p)
                        {
                            //创建匿名类型的来源映射
                            //Debug.WriteLine($"创建匿名类型的来源映射: {p.Name}");
                            Context.CreateAnonymousMap(exp.Type, p, member.Name, e.Member.Name);
                            //if (!p.Type.IsAnonymous())
                            //{
                            // member 是从 p.Type中来的
                            //}
                            //else { }
                        }
                        //else if (parent is MemberExpression m) { }
                    }
                }
                ResolvedMembers.Add(exp.Members[i].Name);
                if (Options.SqlType == SqlPartial.Select)
                {
                    Visit(arg);
                    if (UseAs)
                    {
                        Sql.Append($" AS {Database.AttachEmphasis(member.Name)}");
                    }
                }
                else if (Options.SqlType == SqlPartial.Insert)
                {
                    var col = Context.GetTable(exp.Type).Columns.First(c => c.PropertyName == member.Name);
                    Sql.Append($"{Database.AttachEmphasis(col.ColumnName)} = ");
                    Visit(arg);
                }
                else
                {
                    Visit(arg);
                }
            }

            if (i + 1 < exp.Arguments.Count)
            {
                //if (Options.SqlType == SqlPartial.Select)
                //{
                //    Sql.Append(SqlBuilder.N);
                //}
                Sql.Append(", ");
            }
        }
        return null;
    }

    Expression? VisitUnary(UnaryExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: UnaryExpression: {exp}");
        IsNot = exp.NodeType == ExpressionType.Not;
        isVisitConvert = exp.NodeType == ExpressionType.Convert;
        Visit(exp.Operand);
        return null;
    }

    Expression? VisitParameter(ParameterExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: ParameterExpression: {exp}");
        if (Options.SqlType == SqlPartial.Select)
        {
            var alias = Context.GetTable(exp).Alias;
            Sql.Append($"{Database.AttachEmphasis(alias!)}.*");
            UseAs = false;
            //foreach (var item in alias.Columns)
            //{
            //    var prop = Expression.Property(exp, item.PropertyName);
            //    Visit(prop);
            //    if (UseAs)
            //    {
            //        Sql.Append()
            //    }
            //}
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
                    Sql.Append($"{Database.AttachEmphasis(table.Alias)}.{Database.AttachEmphasis(col.ColumnName)}");
                }
                else
                {
                    Sql.Append($"{Database.AttachEmphasis(col.ColumnName)}");
                }
            }
        }

        return null;
    }

    Expression? VisitMemberInit(MemberInitExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: MemberInitExpression: {exp}");
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
                    Sql.Append($" AS {Database.AttachEmphasis(memberAssign.Member.Name)}");
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
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: MemberExpression: {exp}");
        if (bodyExpression?.NodeType == ExpressionType.MemberAccess)
        {
            ResolvedMembers.Add(exp.Member.Name);
        }
        if (exp.Expression?.NodeType == ExpressionType.Parameter)
        {
            //var eType = exp.Type;
            //if (eType.IsGenericType && eType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //{
            //    eType = eType.GetGenericArguments()[0];
            //}

            var paramExp = exp.Expression as ParameterExpression;
            var pType = paramExp?.Type;
            if (pType.IsAnonymous() == true)
            {
                if (Context.TryGetAnonymousInfo(paramExp, exp.Member.Name, out var i))
                {
                    paramExp = i!.ParameterExp;
                }
            }
            //var memberType = exp.Member!.DeclaringType!;
            var name = exp.Member.Name;
            var table = Context.GetTable(paramExp!);
            var col = table.GetColumn(name)!;
            if (col.IsNavigate == true)
            {
                UseNavigate = true;
                NavigateMembers.Add(col.PropertyName);
                if (NavigateDeep == 0)
                {
                    Members.Clear();
                }
                else
                {
                    return null;
                }
            }
            if (Members.Count > 0)
            {
                //// g.Group.Property
                //var member = Members.Pop();
                //// 如果是聚合的属性，该字段的所属表就是exp.Member.DeclaringType，否则走下面的逻辑
                if (col.IsAggregated)
                {

                }
                //    memberType = member.DeclaringType!;
                //name = member.Name;

                //if (memberType.IsAnonymous() && Context.GetAnonymousInfo(memberType, name, out var m))
                //{
                //    memberType = m!.DeclaringType!;
                //    name = m.Name;
                //}
                //table = Context.GetTable(paramExp!);
                //col = table.GetColumn(name)!;
            }
            if (!table.TableEntityInfo.IsAnonymousType)
            {
                UseAs = col.ColumnName != col.PropertyName;
            }
            if (Options.RequiredTableAlias)
            {
                Sql.Append($"{Database.AttachEmphasis(table.Alias!)}.{Database.AttachEmphasis(col.ColumnName)}");
            }
            else
            {
                Sql.Append($"{Database.AttachEmphasis(col.ColumnName)}");
            }
            Members.Clear();
            return null;
        }
        Members.Push(exp.Member);
        return exp.Expression ?? Expression.Constant(exp.Type.TypeDefaultValue(), exp.Type);
    }

    Expression? VisitConstant(ConstantExpression exp)
    {
        Debug.WriteLine($"{Options.SqlAction} {Options.SqlType}: ConstantExpression: {exp}");
        var value = exp.Value;
        if (Members.Count > 0 && value != null)
        {
            value = GetValue(Members, value, out var name);
            VariableValue(value, name);
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
                Sql.Append("NULL");
                return;
            }

            if (v is IEnumerable enumerable && v is not string)
            {
                var names = new List<string>();
                int i = 0;
                foreach (var item in enumerable)
                {
                    var n = $"{bn}_{i}";
                    var parameterName = AddDbParameter(n, item);
                    names.Add(parameterName);
                    i++;
                }
                Sql.Append(string.Join(", ", names));
            }
            else
            {
                if (v is bool b)
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
                    var parameterName = AddDbParameter(bn, value);
                    Sql.Append(parameterName);
                }
            }
        }

        void ConstraintValue(object? v)
        {
            if (v == null)
            {
                Sql.Append("NULL");
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
                    Sql.Append($"{v}");
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
                    //var parameterName = AddDbParameter("Const", value);
                    Sql.Append($"'{v}'");
                }
            }
            if (Options.SqlAction == SqlAction.Select)
            {
                UseAs = true;
            }
        }
    }

    private string AddDbParameter(string name, object v)
    {
        if (v is string s && !Options.Parameterized)
        {
            return $"'{s}'";
        }
        // TODO 非参数化模式
        var parameterName = $"{Context.ParameterPrefix}{name}_{Options.ParameterIndex}";
        DbParameters.Add(parameterName, v);
        Options.ParameterIndex++;
        return Database.AttachPrefix(parameterName);
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfos">成员信息</param>
    /// <param name="compilerVar">编译器变量值</param>
    /// <param name="memberName">成员名称</param>
    /// <returns></returns>
    public static object GetValue(Stack<MemberInfo> memberInfos, object compilerVar, out string memberName)
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

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="memberInfo">成员信息</param>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static object GetValue(MemberInfo memberInfo, object obj)
    {
        if (obj == null)
        {
            return null!;
        }
        if (memberInfo.MemberType == MemberTypes.Property)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            return propertyInfo!.GetValue(obj)!;
        }
        else if (memberInfo.MemberType == MemberTypes.Field)
        {
            var fieldInfo = memberInfo as FieldInfo;
            return fieldInfo!.GetValue(obj)!;
        }
        return new NotSupportedException($"不支持获取 {memberInfo.MemberType} 类型值.");
    }
}
