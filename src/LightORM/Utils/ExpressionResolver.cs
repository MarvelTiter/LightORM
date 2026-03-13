using LightORM.Extension;
using LightORM.Performances;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
namespace LightORM;

internal static class ExpressionExtensions
{
    private static readonly ConcurrentDictionary<(SqlAction, ulong), ExpressionResolvedResult> expressionResolvedResultCache = new();
    public static ExpressionResolvedResult Resolve(this Expression? expression, SqlResolveOptions options, ResolveContext context)
    {
        //resolve.Visit(expression);
        //return new ExpressionResolvedResult(resolve);
        //var resolve = new ExpressionResolver(options, context);
        bool enableCache = ExpressionSqlOptions.Instance.Value.EnableExpressionCache;
        ulong hash = 0;
        if (enableCache)
        {
            hash = ExpressionHasher.Default.ComputeHash64(expression);
            Debug.WriteLineIf(ShowExpressionHashCodeDebugInfo, $"hashcocde: {hash}");
        }
        var key = (options.SqlAction, hash);
        if (enableCache && expressionResolvedResultCache.TryGetValue(key, out var result))
        {
            //result.DbParameters
            if (result.NeedToExtractValues)
            {
                var parameters = ExpressionValueExtract.Default.Extract(expression, options, context);
                return result with { DbParameters = parameters };
            }
            return result;
        }
        else
        {
            var resolver = ExpressionResolverPool.Rent(options, context);
            try
            {
                resolver.Visit(expression);
                result = new(resolver)
                {
                    NeedToExtractValues = resolver.ContainVariable
                };
                expressionResolvedResultCache.TryAdd(key, result);
                if (resolver.DbParameters.Count > 0)
                {
                    return result with { DbParameters = [.. resolver.DbParameters] };
                }
                return result;
            }
            finally
            {
                ExpressionResolverPool.Return(resolver);
            }
        }
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

    public static string OperatorParser(this ExpressionType expressionNodeType)
    {
        return expressionNodeType switch
        {
            ExpressionType.And or
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Equal => " = ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.NotEqual => " <> ",
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
    public SqlResolveOptions Options { get; set; } = options;
    public ResolveContext Context { get; set; } = context;
    public HashSet<ResolvedValueInfo> DbParameters { get; set; } = [];
    public StringBuilder Sql { get; set; } = new StringBuilder(128);
    public Stack<MemberPathInfo> Members { get; set; } = [];
    public List<string> ResolvedMembers { get; set; } = [];
    public List<WindowFnSpecification>? WindowFnPartials { get; set; }
    public bool IsNot { get; set; }
    public bool UseNavigate { get; set; }
    public int NavigateDeep { get; set; }
    public int Level => Context.Level;
    internal List<string> NavigateMembers { get; set; } = [];
    public Dictionary<string, Expression?>? ExpStores { get; set; }
    public Expression[]? NavigateWhereExpression { get; set; }
    public string? MemberOfNavigateMember { get; set; }
    private ISqlMethodResolver MethodResolver => Context.Database.MethodResolver;
    private ICustomDatabase Database => Context.Database;
    public Expression? Body => bodyExpression;
    public bool ContainVariable { get; set; }
    public ReadOnlyCollection<ParameterExpression>? Parameters
    {
        get => parametersExpression;
        set => parametersExpression = value;
    }

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
    //string? lastResolvedColumnName;
    bool useAs = true;
    public bool UseAs
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


    bool isVisitConvert;
    public bool IsVisitConvert
    {
        get => isVisitConvert;
        set => isVisitConvert = value;
    }
    bool specificHandleJson;

    Indexer resolvedIndex = default;
    Indexer GetResolvedIndex(bool shouldReset = true)
    {
        try
        {
            return resolvedIndex;
        }
        finally
        {
            if (shouldReset)
                resolvedIndex = default;
        }
    }

    string? resolvedPropertyName = null;
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
            resolvedIndex = ResolveHelper.ExtractInstanceValue<int>(exp.Right);
            //var array = ResolveHelper.ExtractInstanceValueWithName<Array>(exp.Left, out var name);
            //var arrayValue = array!.GetValue(index);
            ////var pname = ResolveHelper.FormatDbParameterName(Context, Options, $"Arr{index}", ref parameterPositionIndex);
            //var pname = $"{Context.ParameterPrefix}{name}";
            //Sql.Append(pname);
            //DbParameters.Add(new(pname, arrayValue, ExpValueType.Other, resolvedPropertyName));
            //ContainVariable = true;
            //return null;
            return exp.Left;
        }
        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join || Options.SqlType == SqlPartial.Select)
        {
            Sql.Append('(');
        }
        resolvedPropertyName = null;
        Visit(exp.Left);
        var op = exp.NodeType.OperatorParser();
        Sql.Append(op);
        Visit(exp.Right);
        resolvedPropertyName = null;

        if (Options.SqlType == SqlPartial.Where || Options.SqlType == SqlPartial.Join || Options.SqlType == SqlPartial.Select)
        {
            Sql.Append(')');
        }
        return null;
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
        // 索引器
        if (exp.Method.Name.Equals("get_Item"))
        {
            if (exp.Arguments.Count > 0)
            {
                Indexer newIndex;
                if (exp.Arguments[0].Type == typeof(int))
                {
                    newIndex = ResolveHelper.ExtractInstanceValue<int>(exp.Arguments[0]);
                }
                else if (exp.Arguments[0].Type == typeof(string))
                {
                    newIndex = ResolveHelper.ExtractInstanceValue<string>(exp.Arguments[0]);
                }
                else
                {
                    throw new LightOrmException("当前仅支持int和string类型的索引器解析");
                }
                if (resolvedIndex.HasValue)
                {
                    resolvedIndex.Combine(newIndex);
                }
                else
                {
                    resolvedIndex = newIndex;
                }
            }

            Visit(exp.Object);
        }
        else
        {
            if (exp.Method.Name == "op_Implicit" && exp.Method.IsSpecialName)
            {
                return exp.Arguments[0];
            }
            Members.Clear();
            if (exp.Method.Name == "JsonQuery")
            {
                specificHandleJson = true;
            }
            MethodResolver.Resolve(this, exp);
            specificHandleJson = false;
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
        IsVisitConvert = exp.NodeType == ExpressionType.Convert;
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
            if (IsVisitConvert && Members.Count > 0)
            {
                IsVisitConvert = false;
                var member = Members.Pop();
                var table = Context.GetTable(exp);
                var col = table.GetColumn(member.Member.Name)!;
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
                        MemberOfNavigateMember = Members.Pop().Member.Name;
                    }
                    Members.Clear();
                }
                return null;
            }

            // 表达式扁平化处理后，只有Json属性和聚合属性才有 p.XXX.XXX
            if (col.IsJsonColumn && !specificHandleJson)
            {
                // 处理Json属性
                var index = GetResolvedIndex();
                if (index.HasValue)
                {
                    Members.Push(new(null!) { Root = true, IndexValue = index });
                }
                Context.Database.HandleJsonColumn(new(Sql, col, Members, Context, Options, table));
            }
            else
            {
                if (Members.Count > 0)
                {
                    if (Members.Count > 1)
                    {
                        throw new LightOrmException("聚合属性只能嵌套1层");
                    }
                    if (col.IsAggregated)
                    {
                        name = Members.Pop().Member.Name;
                        col = table.GetColumnInfo(name);
                    }
                }

                resolvedPropertyName = col.PropertyName;
                if (Options.RequiredTableAlias)
                {
                    Sql.Append(table.Alias);
                    Sql.Append('.');
                    //Sql.Append($"{table.Alias}.{Database.AttachEmphasis(col.ColumnName)}");
                }
                Sql.AppendEmphasis(col.ColumnName, Database);
                //lastResolvedColumnName = col.ColumnName;
            }
            if (Options.SqlType == SqlPartial.Where)
            {
                ResolvedMembers.Add(col.PropertyName);
            }
            Members.Clear();
            return null;
        }
        var isEndWithConst = EndWithConstant(exp.Expression);
        // 检查exp.Expression是否为常量结尾
        // 如果是，说明resolvedIndex有可能会再次被使用，这里不能重置，需要使用完成后再重置
        Members.Push(new(exp.Member) { IndexValue = GetResolvedIndex(!isEndWithConst) });
        return exp.Expression ?? Expression.Constant(exp.Type.TypeDefaultValue(), exp.Type);
    }

    Expression? VisitConstant(ConstantExpression exp)
    {
        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ConstantExpression: {exp}");
        var value = exp.Value;
        if (Members.Count > 0)
        {
            var v = ResolveHelper.GetValueByExpression(Members, value, out var propNames);
            var pn = $"{Context.ParameterPrefix}{propNames}";
            Sql.Append(pn);
            VariableValue(v, pn);
            ContainVariable = true;
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
                DbParameters.Add(new(bn, null, ExpValueType.Null, resolvedPropertyName));
            }
            else if (v is IEnumerable ee && v is not string)
            {
                var index = GetResolvedIndex();
                if (index.HasValue && index.IsIntValue)
                {
                    var indexValue = ee.GetValueByIndex(index.IntValue);
                    DbParameters.Add(new(bn, indexValue, ExpValueType.Other, resolvedPropertyName));
                }
                else
                {
                    DbParameters.Add(new(bn, v, ExpValueType.Collection, resolvedPropertyName));
                }
            }
            else if (v is bool)
            {
                if (IsNot)
                {
                    DbParameters.Add(new(bn, v, ExpValueType.BooleanReverse, resolvedPropertyName));
                    IsNot = false;
                }
                else
                {
                    DbParameters.Add(new(bn, v, ExpValueType.Boolean, resolvedPropertyName));
                }
            }
            else
            {
                DbParameters.Add(new(bn, v, ExpValueType.Other, resolvedPropertyName));
            }
        }

        void ConstraintValue(object? v)
        {
            if (v == null)
            {
                var name = Guid.NewGuid().ToString("N").Substring(8);
                Sql.Append(name);
                DbParameters.Add(new(name, null, ExpValueType.Null, resolvedPropertyName));
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

    private static bool EndWithConstant(Expression? expression)
    {
        Expression? current = expression;

        while (current is MemberExpression memberExpr)
        {
            current = memberExpr.Expression;
        }
        return current is ConstantExpression;
    }
}
