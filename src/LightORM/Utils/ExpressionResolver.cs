using LightORM.Extension;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Diagnostics;
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
            UsedTables = resolve.UsedTables,
            UseNavigate = resolve.UseNavigate,
            NavigateDeep = resolve.NavigateDeep,
            NavigateMembers = resolve.NavigateMembers,
        };
    }

    //public static ITableEntityInfo GetTable(this ExpressionResolver resolver, Type type)
    //{
    //    var table = resolver.Context?.SelectedTables.FirstOrDefault(t => t.Type == type || type.IsAssignableFrom(t.Type));
    //    table ??= TableContext.GetTableInfo(type);
    //    //throw new LightOrmException($"当前作用域中未找到类型`{type.Name}`的ITableEntityInfo");
    //    return table;
    //}

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
    StringBuilder Sql { get; }
    SqlResolveOptions Options { get; }
    Expression? Visit(Expression? expression);
}

public class ExpressionResolver(SqlResolveOptions options, ResolveContext context) : IExpressionResolver
{
    public SqlResolveOptions Options { get; } = options;
    public ResolveContext Context { get; } = context;
    public Dictionary<string, object> DbParameters { get; set; } = [];
    public StringBuilder Sql { get; set; } = new StringBuilder();
    public Stack<MemberInfo> Members { get; set; } = [];
    public List<string> ResolvedMembers { get; set; } = [];
    public List<ITableEntityInfo> UsedTables { get; set; } = [];
    public bool IsNot { get; set; }
    public bool UseNavigate { get; set; }
    public int NavigateDeep { get; set; }
    internal List<string> NavigateMembers { get; set; } = [];
    ISqlMethodResolver MethodResolver => Context.Database.MethodResolver;
    ICustomDatabase Database => Context.Database;
    public Expression? Visit(Expression? expression)
    {
        System.Diagnostics.Debug.WriteLine($"Current Expression: {expression}");
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
        bodyExpression = exp.Body;
        foreach (var item in exp.Parameters)
        {
            Context?.AddSelectedTable(item);
        }
        return bodyExpression;
    }

    Expression? VisitBinary(BinaryExpression exp)
    {
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
            Sql.Append("( ");
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
            Sql.Append(" )");
        }

        return null;
    }

    Expression? VisitConditional(ConditionalExpression exp)
    {
        //exp.
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
        Members.Clear();
        if (exp.Method.Name.Equals("get_Item") && (exp.Method.DeclaringType?.FullName?.StartsWith("System.Collections.Generic") ?? false))
        {

        }
        else
        {
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
        for (int i = 0; i < exp.Arguments.Count; i++)
        {
            var member = exp.Members![i];
            var arg = exp.Arguments[i];
            if (member.Name.StartsWith("Tb") && member is PropertyInfo prop)
            {
                ParameterExpression p = Expression.Parameter(prop.PropertyType, member.Name);
                Visit(p);
                UseAs = true;
            }
            else
            {
                if (member.DeclaringType.IsAnonymous())
                {
                    if (arg.NodeType == ExpressionType.MemberAccess && arg is MemberExpression e)
                    {
                        var parent = e.Expression;
                        if (parent is ParameterExpression p)
                        {
                            if (!p.Type.IsAnonymous())
                            {
                                //创建匿名类型的来源映射
                                // member 是从 p.Type中来的
                                Context.CreateAnonymousMap(exp.Type, p.Type, member.Name, e.Member.Name);
                            }
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
                        Sql.Append($" AS {Database.AttachEmphasis(member.Name)}");
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
        IsNot = exp.NodeType == ExpressionType.Not;
        isVisitConvert = exp.NodeType == ExpressionType.Convert;
        Visit(exp.Operand);
        return null;
    }
    
    Expression? VisitParameter(ParameterExpression exp)
    {
        //ps.Add(new
        //{
        //    Alias = exp.Name,
        //    exp.Type,
        //});
        Debug.WriteLine($"{Options.SqlAction}:{Options.SqlType} VisitParameter: {exp}");
        if (Options.SqlType == SqlPartial.Select)
        {
            var alias = Context.GetTable(exp.Type).Alias;
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
            var table = Context.GetTable(exp.Type);
            foreach (var item in table.Columns)
            {
                var prop = Expression.Property(exp, item.PropertyName);
                Visit(prop);
                Sql.Append(", ");
            }
            Sql.Remove(Sql.Length - 2, 2);
        }
        else
        {
            if (isVisitConvert && Members.Count > 0)
            {
                isVisitConvert = false;
                var member = Members.Pop();
                var col = Context.GetTable(member.DeclaringType!).Columns.First(c => c.PropertyName == member.Name);
                if (Options.RequiredTableAlias)
                {
                    Sql.Append($"{Database.AttachEmphasis(col.Table.Alias!)}.{Database.AttachEmphasis(col.ColumnName)}");
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
        if (bodyExpression?.NodeType == ExpressionType.MemberAccess)
        {
            ResolvedMembers.Add(exp.Member.Name);
        }
        if (exp.Expression?.NodeType == ExpressionType.Parameter)
        {
            var type = exp.Type;
            var parent = exp.Expression as ParameterExpression;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            //if (Options.SqlType == SqlPartial.Select && exp.Expression is ParameterExpression p)
            //{
            //    //UsedTables.Add(Context.GetTable(p.Type));
            //}
            var memberType = exp.Member!.DeclaringType!;
            var name = exp.Member.Name;
            var table = Context.GetTable(memberType);
            var col = table.Columns.First(c => c.PropertyName == name);
            if (col.IsNavigate)
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
                // g.Group.Property
                var member = Members.Pop();
                memberType = member.DeclaringType!;
                name = member.Name;
                if (memberType.IsAnonymous())
                {
                    member = Context.GetAnonymousInfo(memberType, name);
                    memberType = member.DeclaringType!;
                    name = member.Name;
                }
                col = Context.GetTable(memberType).Columns.First(c => c.PropertyName == name);
            }
            UseAs = col.ColumnName != col.PropertyName;
            if (Options.RequiredTableAlias)
            {
                Sql.Append($"{Database.AttachEmphasis(col.Table.Alias!)}.{Database.AttachEmphasis(col.ColumnName)}");
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
        var value = exp.Value;
        if (Members.Count > 0 && value != null)
        {
            value = GetValue(Members, value, out var name);
            if (value == null)
            {
                Sql.Append("NULL");
                return null;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                var names = new List<string>();
                int i = 0;
                foreach (var item in enumerable)
                {
                    var n = $"{name}_{i}";
                    var parameterName = AddDbParameter(n, item);
                    names.Add(parameterName);
                    i++;
                }
                Sql.Append(string.Join(", ", names));
            }
            else
            {
                var parameterName = AddDbParameter(name, value);
                Sql.Append(parameterName);
            }
        }
        else
        {
            if (value == null)
            {
                Sql.Append("NULL");
                return null;
            }
            if (bodyExpression?.NodeType == ExpressionType.Constant && exp.Type == typeof(bool))
            {
                var b = (bool)value;
                Sql.Append("1 = ");
                Sql.Append(b ? "1" : "0");
            }
            else
            {
                if (exp.Type.IsNumber())
                {
                    Sql.Append($"{value}");
                }
                else
                {
                    var parameterName = AddDbParameter("Const", value);
                    Sql.Append(parameterName);
                }
            }
        }
        return null;
    }

    private string AddDbParameter(string name, object v)
    {
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
