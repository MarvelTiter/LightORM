using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces.ExpSql;
using LightORM.Performances;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LightORM.Utils;

internal class ExpressionValueExtract : ExpressionVisitor, IResetable
{
    private readonly List<DbParameterInfo> parameters = [];
    private int parameterIndex = 0;
    private readonly Stack<MemberInfo> members = [];
    public static ExpressionValueExtract Default => ExpressionVisitorPool<ExpressionValueExtract>.Rent();
    private ResolveContext? context;
    private SqlResolveOptions? option;
    public void Reset()
    {
        parameters.Clear();
        parameterIndex = 0;
        members.Clear();
        context = null;
        option = null;
    }
    public List<DbParameterInfo> Extract(Expression? exp)
    {
        try
        {
            if (exp is not null)
            {
                Visit(exp);
            }
            return [..parameters];
        }
        finally
        {
            ExpressionVisitorPool<ExpressionValueExtract>.Return(this);
        }
    }
    public List<DbParameterInfo> Extract(Expression? exp, SqlResolveOptions option, ResolveContext context)
    {
        try
        {
            if (exp is not null)
            {
                this.context = context;
                this.option = option;
                Visit(exp);
            }
            return [..parameters];
        }
        finally
        {
            ExpressionVisitorPool<ExpressionValueExtract>.Return(this);
        }
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.NodeType == ExpressionType.ArrayIndex)
        {
            //var index = Convert.ToInt32(Expression.Lambda(node.Right).Compile().DynamicInvoke());
            var index = ResolveHelper.ExtractInstanceValue<int>(node.Right);
            var array = ResolveHelper.ExtractInstanceValue<Array>(node.Left);
            var arrayValue = array!.GetValue(index);
            var pname = ResolveHelper.FormatDbParameterName(context, option, $"Arr{index}", ref parameterIndex);
            parameters.Add(new(pname, arrayValue, ExpValueType.Other));
        }
        else
        {
            Visit(node.Left);
            Visit(node.Right);
        }
        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        Visit(node.Operand);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression?.NodeType == ExpressionType.Parameter)
        {
            members.Clear();
        }
        else
        {
            members.Push(node.Member);
        }
        Visit(node.Expression);
        return node;
    }


    protected override Expression VisitConstant(ConstantExpression node)
    {
        var value = node.Value;
        if (members.Count > 0 && value != null)
        {
            value = ResolveHelper.GetValueByExpression(members, value, out var name);
            var bn = ResolveHelper.FormatDbParameterName(context, option, name, ref parameterIndex);
            VariableValue(value, bn);
        }

        return node;

        void VariableValue(object? v, string bn)
        {
            if (v == null)
            {
                parameters.Add(new DbParameterInfo(bn, null, ExpValueType.Null));
                return;
            }

            if (v is IEnumerable && v is not string)
            {
                parameters.Add(new(bn, v, ExpValueType.Collection));
            }
            else if (v is bool)
            {
                parameters.Add(new(bn, v, ExpValueType.Boolean));
            }
            else
            {
                parameters.Add(new(bn, v, ExpValueType.Other));
            }
        }
    }
}


//internal class ExpressionValueResolver(SqlResolveOptions options, ResolveContext context)
//{
//    public SqlResolveOptions Options { get; set; } = options;
//    public ResolveContext Context { get; set; } = context;
//    public List<DbParameterInfo> DbParameters { get; set; } = [];
//    public Stack<MemberInfo> Members { get; set; } = [];
//    public List<string> ResolvedMembers { get; set; } = [];
//    public List<WindowFnSpecification>? WindowFnPartials { get; set; }
//    public bool IsNot { get; set; }
//    public bool UseNavigate { get; set; }
//    public int NavigateDeep { get; set; }
//    public int Level => Context.Level;
//    internal List<string> NavigateMembers { get; set; } = [];
//    public Dictionary<string, Expression?>? ExpStores { get; set; }
//    public Expression? NavigateWhereExpression { get; set; }
//    public string? MemberOfNavigateMember { get; set; }
//    public Expression? Body => bodyExpression;
//    public bool ContainVariable { get; set; }
//    public ReadOnlyCollection<ParameterExpression>? Parameters
//    {
//        get => parametersExpression;
//        set => parametersExpression = value;
//    }

//    public Expression? Visit(Expression? expression)
//    {
//        //Debug.Write($"");
//        return expression switch
//        {
//            LambdaExpression => Visit(VisitLambda((LambdaExpression)expression)),
//            BinaryExpression => Visit(VisitBinary((BinaryExpression)expression)),
//            ConditionalExpression => Visit(VisitConditional((ConditionalExpression)expression)),
//            MethodCallExpression => Visit(VisitMethodCall((MethodCallExpression)expression)),
//            NewArrayExpression => Visit(VisitNewArray((NewArrayExpression)expression)),
//            NewExpression => Visit(VisitNew((NewExpression)expression)),
//            UnaryExpression => Visit(VisitUnary((UnaryExpression)expression)),
//            ParameterExpression => Visit(VisitParameter((ParameterExpression)expression)),
//            MemberInitExpression => Visit(VisitMemberInit((MemberInitExpression)expression)),
//            MemberExpression => Visit(VisitMember((MemberExpression)expression)),
//            ConstantExpression => Visit(VisitConstant((ConstantExpression)expression)),
//            _ => null
//        };
//    }

//    private const string AS_LITERAL = " AS ";
//    Expression? bodyExpression;
//    ReadOnlyCollection<ParameterExpression>? parametersExpression;
//    //string? lastResolvedColumnName;
//    bool useAs = true;
//    public bool UseAs
//    {
//        get
//        {
//            if (!useAs)
//            {
//                useAs = true;
//                return false;
//            }
//            return useAs && Options.UseColumnAlias;
//        }
//        set => useAs = value;
//    }

//    bool resolveNullValue;
//    public bool ResolveNullValue
//    {
//        get
//        {
//            if (resolveNullValue)
//            {
//                resolveNullValue = false;
//                return true;
//            }
//            return resolveNullValue;
//        }
//        set => resolveNullValue = value;
//    }

//    bool isVisitConvert;
//    public bool IsVisitConvert
//    {
//        get => isVisitConvert;
//        set => isVisitConvert = value;
//    }

//    int parameterPositionIndex = 0;
//    public int ParameterPositionIndex { get => parameterPositionIndex; set => parameterPositionIndex = value; }

//    Expression? VisitLambda(LambdaExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: LambdaExpression: {exp}");
//        bodyExpression = exp.Body;
//        parametersExpression = exp.Parameters;
//        //Context.Tables.Clear();
//        for (int i = 0; i < exp.Parameters.Count; i++)
//        {
//            ParameterExpression? item = exp.Parameters[i];
//            Context.HandleParameterExpression(item, i);
//        }
//        return bodyExpression;
//    }

//    Expression? VisitBinary(BinaryExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: BinaryExpression: {exp}");
//        // 数组访问
//        if (exp.NodeType == ExpressionType.ArrayIndex)
//        {
//            var index = ExtractInstanceValue<int>(exp.Right);
//            var array = ExtractInstanceValue<Array>(exp.Left);
//            var arrayValue = array!.GetValue(index);
//            var pname = FormatDbParameterName(Context, Options, $"Arr{index}", ref parameterPositionIndex);
//            DbParameters.Add(new(pname, arrayValue, ExpValueType.Other));
//            return null;
//        }

//        Visit(exp.Left);
//        Visit(exp.Right);

//        return null;

//        // 尝试将表达式求值为常量（仅支持 Constant 和 简单 Member 访问）
//        static T ExtractInstanceValue<T>(Expression expression)
//        {
//            if (expression is ConstantExpression ce && ce.Value is T index)
//            {
//                return index;
//            }
//            var members = new Stack<MemberInfo>();
//            Expression? current = expression;

//            // 向下遍历，收集 MemberInfo
//            while (current is MemberExpression memberExpr)
//            {
//                members.Push(memberExpr.Member);
//                current = memberExpr.Expression;
//            }
//            object? value;

//            if (current is ConstantExpression constExpr)
//            {
//                value = constExpr.Value;
//            }
//            else if (current is null)
//            {
//                value = null;
//            }
//            else
//            {
//                throw new LightOrmException($"数组索引表达式必须以常量或者Null结尾，但得到: {current?.GetType().Name}: {current}");
//            }

//            value = GetValue(members, value);

//            if (value is T t)
//            {
//                return t;
//            }
//            throw new LightOrmException($"尝试获取类型{typeof(T)}的值，实际类型: {value?.GetType()}");
//        }
//    }

//    Expression? VisitConditional(ConditionalExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ConditionalExpression: {exp}");
//        Visit(exp.Test);
//        Visit(exp.IfTrue);
//        Visit(exp.IfFalse);
//        return null;
//    }

//    Expression? VisitMethodCall(MethodCallExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: MethodCallExpression: {exp}");
//        Members.Clear();
//        if (exp.Method.Name.Equals("get_Item") && (exp.Method.DeclaringType?.FullName?.StartsWith("System.Collections.Generic") ?? false))
//        {

//        }
//        else
//        {
//            if (exp.Method.Name == "op_Implicit" && exp.Method.IsSpecialName)
//            {
//                return exp.Arguments[0];
//            }
//            MethodResolver.Resolve(this, exp);
//        }
//        return null;
//    }

//    Expression? VisitNewArray(NewArrayExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: NewArrayExpression: {exp}");
//        for (int i = 0; i < exp.Expressions.Count; i++)
//        {
//            Visit(exp.Expressions[i]);
//        }
//        return null;
//    }

//    Expression? VisitNew(NewExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: NewExpression: {exp}");
//        for (int i = 0; i < exp.Arguments.Count; i++)
//        {
//            var arg = exp.Arguments[i];
//            Visit(arg);
//        }
//        return null;
//    }

//    Expression? VisitUnary(UnaryExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: UnaryExpression: {exp}");
//        IsNot = exp.NodeType == ExpressionType.Not;
//        IsVisitConvert = exp.NodeType == ExpressionType.Convert;
//        Visit(exp.Operand);
//        return null;
//    }

//    Expression? VisitParameter(ParameterExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ParameterExpression: {exp}");
//        if (Options.SqlType == SqlPartial.Select)
//        {
//        }
//        else if (Options.SqlAction == SqlAction.Insert)
//        {
//            var table = Context.GetTable(exp);
//            foreach (var item in table.TableEntityInfo.Columns)
//            {
//                var prop = Expression.Property(exp, item.PropertyName);
//                Visit(prop);
//            }
//        }
//        else
//        {
//            if (IsVisitConvert && Members.Count > 0)
//            {
//                IsVisitConvert = false;
//                _ = Members.Pop();
//            }
//        }

//        return null;
//    }

//    Expression? VisitMemberInit(MemberInitExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: MemberInitExpression: {exp}");
//        for (int i = 0; i < exp.Bindings.Count; i++)
//        {
//            if (exp.Bindings[i].BindingType != MemberBindingType.Assignment)
//            {
//                continue;
//            }
//            if (exp.Bindings[i] is not MemberAssignment memberAssign)
//            {
//                continue;
//            }
//            if (Options.SqlType == SqlPartial.Select)
//            {
//                Visit(memberAssign.Expression);
//            }
//        }
//        return null;
//    }

//    Expression? VisitMember(MemberExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: MemberExpression: {exp}");
//        if (bodyExpression?.NodeType == ExpressionType.MemberAccess)
//        {
//            ResolvedMembers.Add(exp.Member.Name);
//        }
//        if (exp.Expression?.NodeType == ExpressionType.Parameter)
//        {

//            var paramExp = exp.Expression as ParameterExpression;
//            //var pType = paramExp?.Type;

//            var name = exp.Member.Name;
//            var table = Context.GetTable(paramExp!);
//            var col = table.GetColumn(name)!;
//            if (col.IsNavigate == true)
//            {
//                UseNavigate = true;
//                NavigateMembers.Add(col.PropertyName);
//                if (NavigateDeep == 0)
//                {
//                    if (Members.Count > 0)
//                    {
//                        //ResolvedMembers.Add(Members.Pop().Name);
//                        MemberOfNavigateMember = Members.Pop().Name;
//                    }
//                    Members.Clear();
//                }
//                //else
//                //{
//                //    return null;
//                //}
//                return null;
//            }
//            // TODO 表达式扁平化处理后，只有聚合属性才有 p.XXX.XXX ?
//            if (Members.Count > 0)
//            {
//                if (col.IsAggregated)
//                {
//                    name = Members.Pop().Name;
//                    col = table.GetColumnInfo(name);
//                }
//            }

//            if (Options.SqlType == SqlPartial.Where)
//            {
//                ResolvedMembers.Add(col.PropertyName);
//            }
//            Members.Clear();
//            return null;
//        }
//        Members.Push(exp.Member);
//        return exp.Expression ?? Expression.Constant(exp.Type.TypeDefaultValue(), exp.Type);
//    }

//    Expression? VisitConstant(ConstantExpression exp)
//    {
//        Debug.WriteLineIf(ShowExpressionResolveDebugInfo, $"{Options.SqlAction} {Options.SqlType}: ConstantExpression: {exp}");
//        var value = exp.Value;
//        if (Members.Count > 0)
//        {
//            //value = GetValue(Members, value, out var name);
//            //VariableValue(value, name);
//            var v = GetValueByExpression(Members, value, out var propNames);
//            var pn = FormatDbParameterName(Context, Options, propNames, ref parameterPositionIndex);
//            VariableValue(v, pn);
//        }
//        else
//        {
//            ConstraintValue(value);
//        }
//        return null;

//        void VariableValue(object? v, string bn)
//        {
//            if (v == null)
//            {
//                DbParameters.Add(new(bn, null, ExpValueType.Null));
//            }
//            else if (v is IEnumerable && v is not string)
//            {
//                DbParameters.Add(new(bn, v, ExpValueType.Collection));
//            }
//            else if (v is bool)
//            {
//                if (IsNot)
//                {
//                    DbParameters.Add(new(bn, v, ExpValueType.BooleanReverse));
//                    IsNot = false;
//                }
//                else
//                {
//                    DbParameters.Add(new(bn, v, ExpValueType.Boolean));
//                }
//            }
//            else
//            {
//                DbParameters.Add(new(bn, v, ExpValueType.Other));
//            }
//        }

//        void ConstraintValue(object? v)
//        {
//            if (v == null)
//            {
//                ResolveNullValue = true;
//                return;
//            }
//            if (bodyExpression?.NodeType == ExpressionType.Constant && exp.Type == typeof(bool))
//            {
//            }
//            else
//            {
//                if (exp.Type.IsNumber())
//                {
//                }
//                else if (exp.Type.IsBoolean() && v is bool _)
//                {
//                    if (IsNot)
//                    {
//                        IsNot = false;
//                    }
//                    else
//                    {
//                    }
//                }
//                else
//                {
//                }
//            }
//        }
//    }

//    public static string FormatDbParameterName(ResolveContext? context, SqlResolveOptions? option, string name, ref int index)
//    {
//        var p = $"{context?.ParameterPrefix}{name}_{option?.ParameterPartialIndex}_{index}";
//        index += 1;
//        return p;
//    }

//    //public static void FormatDbParameterName(StringBuilder sql, ResolveContext? context, SqlResolveOptions? option, string name, ref int index)
//    //{
//    //    //var p = $"{context?.ParameterPrefix}{name}_{option?.ParameterPartialIndex}_{index}";
//    //    //index += 1;
//    //    //return p;
//    //    sql.Append(context?.ParameterPrefix);
//    //    sql.Append(name);
//    //    sql.Append('_');
//    //    sql.Append(option?.ParameterPartialIndex);
//    //    sql.Append('_');
//    //    index += 1;
//    //    sql.Append(index);
//    //}

//    /// <summary>
//    /// 获取值
//    /// </summary>
//    /// <param name="memberInfos">成员信息</param>
//    /// <param name="compilerVar">编译器变量值</param>
//    /// <param name="memberName">成员名称</param>
//    /// <returns></returns>
//    [Obsolete]
//    public static object? GetValue(Stack<MemberInfo> memberInfos, object? compilerVar, out string memberName)
//    {
//        var names = new List<string>();
//        while (memberInfos.Count > 0)
//        {
//            var item = memberInfos.Pop();
//            if (!item.Name.StartsWith("CS$<>8__locals"))
//            {
//                names.Add(item.Name);
//            }

//            compilerVar = GetValue(item, compilerVar);
//        }
//        memberName = string.Join("_", names);
//        return compilerVar;
//    }
//    public static object? GetValue(Stack<MemberInfo> memberInfos, object? compilerVar) => GetValueByExpression(memberInfos, compilerVar, out _);

//    /// <summary>
//    /// 获取值
//    /// </summary>
//    /// <param name="memberInfo">成员信息</param>
//    /// <param name="obj">对象</param>
//    /// <returns></returns>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    [Obsolete]
//    public static object? GetValue(MemberInfo memberInfo, object? obj)
//    {
//        return memberInfo switch
//        {
//            PropertyInfo prop => prop.GetValue(obj),
//            FieldInfo field => field.GetValue(obj),
//            _ => throw new NotSupportedException($"不支持获取 {memberInfo.MemberType} 类型值.")
//        };
//    }

//    private static readonly ConcurrentDictionary<string, Func<object?, object?>> getterCache = [];
//    public static object? GetValueByExpression(Stack<MemberInfo> memberInfos, object? value, out string name)
//    {
//        name = string.Join("_", memberInfos.Where(m => !m.Name.StartsWith("CS$<>8__locals")).Select(m => m.Name));
//        if (value is null) return null;
//        var type = value.GetType();
//        var memberKey = $"{type.FullName}_{name}";
//        var func = getterCache.GetOrAdd(memberKey, _ =>
//        {
//            return CreateGetter(type, memberInfos);
//        });
//        return func.Invoke(value);
//    }

//    private static Func<object?, object?> CreateGetter(Type type, Stack<MemberInfo> memberInfos)
//    {
//        var param = Expression.Parameter(typeof(object), "obj");
//        Expression body = Expression.Convert(param, type);
//        while (memberInfos.Count > 0)
//        {
//            body = Expression.MakeMemberAccess(body, memberInfos.Pop());
//        }
//        body = Expression.Convert(body, typeof(object));
//        var lambda = Expression.Lambda<Func<object?, object?>>(body, param);
//        return lambda.Compile();
//    }
//}

//internal class MethodValueResolver : BaseSqlMethodResolver
//{

//}