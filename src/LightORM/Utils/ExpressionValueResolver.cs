//using LightORM.Extension;
//using System.Reflection;
//using System.Collections;
//namespace LightORM;

//internal class ExpressionValueResolver
//{
//    public static ExpressionValueResolver Default => new ExpressionValueResolver();
//    public Dictionary<string, object> DbParameters { get; set; } = [];
//    public Stack<MemberInfo> Members { get; set; } = [];
//    public Expression? Visit(Expression? expression)
//    {
//        return expression switch
//        {
//            LambdaExpression l => Visit(l.Body),
//            BinaryExpression => Visit(VisitBinary((BinaryExpression)expression)),
//            ConditionalExpression => Visit(VisitConditional((ConditionalExpression)expression)),
//            MethodCallExpression => Visit(VisitMethodCall((MethodCallExpression)expression)),
//            NewArrayExpression => Visit(VisitNewArray((NewArrayExpression)expression)),
//            NewExpression => Visit(VisitNew((NewExpression)expression)),
//            UnaryExpression => Visit(VisitUnary((UnaryExpression)expression)),
//            ParameterExpression => null,
//            MemberInitExpression => null,
//            MemberExpression => Visit(VisitMember((MemberExpression)expression)),
//            ConstantExpression => Visit(VisitConstant((ConstantExpression)expression)),
//            _ => null
//        };
//    }

//    Expression? VisitBinary(BinaryExpression exp)
//    {
//        Visit(exp.Left);
//        Visit(exp.Right);
//        return null;
//    }

//    Expression? VisitConditional(ConditionalExpression exp)
//    {
//        Visit(exp.Test);
//        Visit(exp.IfTrue);
//        Visit(exp.IfFalse);
//        return null;
//    }

//    Expression? VisitMethodCall(MethodCallExpression exp)
//    {
//        Members.Clear();
//        return null;
//    }

//    Expression? VisitNewArray(NewArrayExpression exp)
//    {
//        for (int i = 0; i < exp.Expressions.Count; i++)
//        {
//            Visit(exp.Expressions[i]);
//        }
//        return null;
//    }

//    Expression? VisitNew(NewExpression exp)
//    {
//        for (int i = 0; i < exp.Arguments.Count; i++)
//        {
//            var arg = exp.Arguments[i];
//            Visit(arg);
//        }
//        return null;
//    }

//    Expression? VisitUnary(UnaryExpression exp)
//    {
//        Visit(exp.Operand);
//        return null;
//    }


//    Expression? VisitMember(MemberExpression exp)
//    {
//        if (exp.Expression?.NodeType == ExpressionType.Parameter)
//        {
//            Members.Clear();
//            return null;
//        }
//        Members.Push(exp.Member);
//        return exp.Expression ?? Expression.Constant(exp.Type.TypeDefaultValue(), exp.Type);
//    }

//    Expression? VisitConstant(ConstantExpression exp)
//    {
//        var value = exp.Value;
//        if (Members.Count > 0 && value != null)
//        {
//            value = GetValue(Members, value, out var name);
//            VariableValue(value, name);
//        }

//        return null;

//        void VariableValue(object? v, string bn)
//        {
//            if (v == null)
//            {
//                return;
//            }

//            if (v is IEnumerable enumerable && v is not string)
//            {
//                var names = new List<string>();
//                int i = 0;
//                foreach (var item in enumerable)
//                {
//                    var n = $"{bn}_{i}";
//                    AddDbParameter(n, item);
//                    i++;
//                }
//            }
//            else
//            {
//                AddDbParameter(bn, value);
//            }
//        }
//    }

//    private void AddDbParameter(string name, object v)
//    {
//        var parameterName = $"p{DbParameters.Count}";
//        DbParameters.Add(parameterName, v);
//    }

//    /// <summary>
//    /// 获取值
//    /// </summary>
//    /// <param name="memberInfos">成员信息</param>
//    /// <param name="compilerVar">编译器变量值</param>
//    /// <param name="memberName">成员名称</param>
//    /// <returns></returns>
//    public static object GetValue(Stack<MemberInfo> memberInfos, object compilerVar, out string memberName)
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

//    /// <summary>
//    /// 获取值
//    /// </summary>
//    /// <param name="memberInfo">成员信息</param>
//    /// <param name="obj">对象</param>
//    /// <returns></returns>
//    public static object GetValue(MemberInfo memberInfo, object obj)
//    {
//        if (obj == null)
//        {
//            return null!;
//        }
//        if (memberInfo.MemberType == MemberTypes.Property)
//        {
//            var propertyInfo = memberInfo as PropertyInfo;
//            return propertyInfo!.GetValue(obj)!;
//        }
//        else if (memberInfo.MemberType == MemberTypes.Field)
//        {
//            var fieldInfo = memberInfo as FieldInfo;
//            return fieldInfo!.GetValue(obj)!;
//        }
//        return new NotSupportedException($"不支持获取 {memberInfo.MemberType} 类型值.");
//    }
//}