using LightORM.Cache;
using LightORM.Extension;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;
using LightORM.SqlMethodResolver;
namespace LightORM;

internal static class ExpressionExtensions
{
    public static ExpressionResolvedResult Resolve(this Expression? expression, SqlResolveOptions options, params ITableEntityInfo[] tables)
    {
        var resolve = new ExpressionResolver(options, tables);
        resolve.Visit(expression);
        return new ExpressionResolvedResult
        {
            SqlString = resolve.Sql.ToString(),
            DbParameters = resolve.DbParameters,
            Members = resolve.ResolvedMembers,
            UseNavigate = resolve.UseNavigate,
            NavigateDeep = resolve.NavigateDeep,
            NavigateMembers = resolve.NavigateMembers,
        };
    }

    public static ITableEntityInfo GetTable(this ExpressionResolver resolver, Type type)
    {
        var table = resolver.Tables.FirstOrDefault(t => t.Type == type);
        if (table == null)
        {
            throw new LightOrmException($"当前作用域中未找到类型`{type.Name}`的ITableEntityInfo");
        }
        return table;
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
            _ => throw new NotImplementedException("未实现的节点类型" + expressionNodeType)
        };
    }
}

public interface IExpressionResolver
{
    bool IsNot { get; }
    StringBuilder Sql { get; }
    SqlResolveOptions Options { get; }
    Expression? Visit(Expression? expression);
}

public class ExpressionResolver(SqlResolveOptions options, params ITableEntityInfo[] tables) : IExpressionResolver
{
    public SqlResolveOptions Options { get; } = options;
    public ITableEntityInfo[] Tables { get; } = tables;
    public Dictionary<string, object> DbParameters { get; set; } = [];
    public StringBuilder Sql { get; set; } = new StringBuilder();
    public Stack<MemberInfo> Members { get; set; } = [];
    public List<string> ResolvedMembers { get; set; } = [];
    public bool IsNot { get; set; }
    public bool UseNavigate { get; set; }
    public int NavigateDeep { get; set; }
    internal List<string> NavigateMembers { get; set; } = [];
    public SqlMethod MethodResolver { get; } = options.DbType.GetSqlMethodResolver();
    //public Expression? Resolve(Expression? expression) => Visit(expression);
    public Expression? Visit(Expression? expression)
    {
        System.Diagnostics.Debug.WriteLine($"Current Expression: {expression}");
        return expression switch
        {
            LambdaExpression => Visit(VisitLambda((LambdaExpression)expression)),
            BinaryExpression => Visit(VisitBinary((BinaryExpression)expression)),
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

    Expression? VisitLambda(LambdaExpression exp)
    {
        bodyExpression = exp.Body;

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
            Sql.Append(Options.DbType.AttachPrefix(parameterName));
            return null;
        }
        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join)
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

        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join)
        {
            Sql.Append(" )");
        }

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
            MethodResolver.Invoke(this, exp);
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
            if (member.Name.StartsWith("Tb") && member is PropertyInfo prop)
            {
                ParameterExpression p = Expression.Parameter(prop.PropertyType, member.Name);
                Visit(p);
                NotAs = false;
            }
            else
            {
                ResolvedMembers.Add(exp.Members[i].Name);
                if (Options.SqlType == SqlPartial.Select)
                {
                    Visit(exp.Arguments[i]);
                    if (!NotAs)
                        Sql.Append($" AS {Options.DbType.AttachEmphasis(exp.Members![i].Name)}");
                }
                else if (Options.SqlType == SqlPartial.Insert)
                {
                    var col = this.GetTable(exp.Type).Columns.First(c => c.PropertyName == exp.Members![i].Name);
                    Sql.Append($"{Options.DbType.AttachEmphasis(col.ColumnName)} = ");
                    Visit(exp.Arguments[i]);
                }
                else
                {
                    Visit(exp.Arguments[i]);
                }
            }
            if (i + 1 < exp.Arguments.Count)
            {
                Sql.Append(", \n");
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
    bool notAs;
    bool NotAs
    {
        get
        {
            if (notAs)
            {
                notAs = false;
                return true;
            }
            return notAs;
        }
        set => notAs = value;
    }
    bool isVisitConvert;
    //List<object> ps = [];
    Expression? VisitParameter(ParameterExpression exp)
    {
        //ps.Add(new
        //{
        //    Alias = exp.Name,
        //    exp.Type,
        //});
        if (Options.SqlType == SqlPartial.Select)
        {
            var alias = this.GetTable(exp.Type).Alias;
            Sql.Append($"{Options.DbType.AttachEmphasis(alias!)}.*");
            NotAs = true;
        }
        else
        {
            if (isVisitConvert && Members.Count > 0)
            {
                isVisitConvert = false;
                var member = Members.Pop();
                var col = this.GetTable(member.DeclaringType!).Columns.First(c => c.PropertyName == member.Name);
                if (Options.RequiredTableAlias)
                {
                    Sql.Append($"{Options.DbType.AttachEmphasis(col.Table.Alias!)}.{Options.DbType.AttachEmphasis(col.ColumnName)}");
                }
                else
                {
                    Sql.Append($"{Options.DbType.AttachEmphasis(col.ColumnName)}");
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
            var memberAssign = exp.Bindings[i] as MemberAssignment;
            if (Options.SqlType == SqlPartial.Select)
            {

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
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            var memberType = exp.Member!.DeclaringType!;
            var name = exp.Member.Name;
            //if (exp.Member.Name.StartsWith("Tb") && exp.Member is PropertyInfo prop)
            //{
            //    ParameterExpression p = Expression.Parameter(prop.PropertyType, exp.Member.Name);
            //    Visit(p);
            //}
            var col = this.GetTable(memberType).Columns.First(c => c.PropertyName == name);
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
                // w.Tb1.Property
                var member = Members.Pop();
                memberType = member.DeclaringType!;
                name = member.Name;
                col = this.GetTable(memberType).Columns.First(c => c.PropertyName == name);
            }
            if (Options.RequiredTableAlias)
            {
                Sql.Append($"{Options.DbType.AttachEmphasis(col.Table.Alias!)}.{Options.DbType.AttachEmphasis(col.ColumnName)}");
            }
            else
            {
                Sql.Append($"{Options.DbType.AttachEmphasis(col.ColumnName)}");
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
            //if (value is IList list)
            //{
            //    var names = new List<string>();
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        var n = $"{name}_{i}";
            //        var parameterName = AddDbParameter(n, list[i]!);
            //        names.Add(parameterName);
            //    }
            //    Sql.Append(string.Join(",", names.Select(s => $"{Options.DbType.AttachPrefix(s)}")));
            //}
            //if (value is string str)
            //{
            //    var parameterName = AddDbParameter(name, str);
            //    Sql.Append($"{Options.DbType.AttachPrefix(parameterName)}");
            //}
            //else
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
                Sql.Append(string.Join(", ", names.Select(s => $"{Options.DbType.AttachPrefix(s)}")));
            }
            else
            {
                var parameterName = AddDbParameter(name, value);
                Sql.Append($"{Options.DbType.AttachPrefix(parameterName)}");
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
                    Sql.Append($"{Options.DbType.AttachPrefix(parameterName)}");
                }
            }
        }
        return null;
    }

    private string AddDbParameter(string name, object v)
    {
        var parameterName = $"{name}_{Options.ParameterIndex}";
        DbParameters.Add(parameterName, v);
        Options.ParameterIndex++;
        return parameterName;
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
