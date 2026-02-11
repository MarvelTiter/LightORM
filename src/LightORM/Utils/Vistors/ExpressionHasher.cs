using LightORM.Performances;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LightORM.Utils.Vistors;

//internal class ExpressionHashCreator : ExpressionVisitor, IResetable
//{
//    private readonly StringBuilder sb = new(128);
//    public void Reset()
//    {
//        sb.Clear();
//        sb.EnsureCapacity(128);
//    }

//    public static ExpressionHashCreator Default => ExpressionVisitorPool<ExpressionHashCreator>.Rent();

//    public string Scan(Expression? node)
//    {
//        try
//        {
//            if (node is null) return string.Empty;
//            _ = Visit(node);
//            return sb.ToString();
//        }
//        finally
//        {
//            ExpressionVisitorPool<ExpressionHashCreator>.Return(this);
//        }
//    }

//    protected override Expression VisitBinary(BinaryExpression node)
//    {
//        sb.Append("B:");
//        sb.Append(node.NodeType);
//        sb.Append('(');
//        Visit(node.Left);
//        sb.Append(',');
//        Visit(node.Right);
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitUnary(UnaryExpression node)
//    {
//        sb.Append("U:");
//        sb.Append(node.NodeType);
//        sb.Append('(');
//        Visit(node.Operand);
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitConstant(ConstantExpression node)
//    {
//        sb.Append("CT");
//        return node;
//    }

//    protected override Expression VisitParameter(ParameterExpression node)
//    {
//        sb.Append("P:");
//        // 使用类型名 + 名称（若无名称则用类型）
//        //sb.Append(node.Name ?? node.Type.Name);
//        sb.Append('(');
//        AppendTypeName(sb, node.Type.Name);
//        sb.Append(':');
//        sb.Append(node.Name);
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitMember(MemberExpression node)
//    {
//        sb.Append("M:");
//        sb.Append(node.Member.Name);
//        sb.Append('@');
//        //sb.Append(node.Member.DeclaringType?.Name ?? "null");
//        AppendTypeName(sb, node.Member.DeclaringType?.Name ?? "NULL");
//        sb.Append('(');
//        Visit(node.Expression); // 递归访问对象表达式（可能是参数或另一个成员）
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitMethodCall(MethodCallExpression node)
//    {
//        sb.Append("C:");
//        //sb.Append(node.Method.DeclaringType?.Name ?? "null");
//        AppendTypeName(sb, node.Method.DeclaringType?.Name ?? "NULL");
//        sb.Append('.');
//        sb.Append(node.Method.Name);
//        sb.Append('(');
//        for (int i = 0; i < node.Arguments.Count; i++)
//        {
//            if (i > 0) sb.Append(',');
//            Visit(node.Arguments[i]);
//        }
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitMemberInit(MemberInitExpression node)
//    {
//        sb.Append("I:");
//        AppendTypeName(sb, node.Type.Name);
//        sb.Append('(');
//        for (int i = 0; i < node.Bindings.Count; i++)
//        {
//            if (i > 0) sb.Append(',');
//            var b = node.Bindings[i];
//            sb.Append(b.Member.Name);
//        }
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitNew(NewExpression node)
//    {
//        sb.Append("N:");
//        //sb.Append(node.Constructor?.DeclaringType?.Name ?? "Anonymous");
//        AppendTypeName(sb, node.Constructor?.DeclaringType?.Name ?? "Amo");
//        sb.Append('(');
//        for (int i = 0; i < node.Arguments.Count; i++)
//        {
//            if (i > 0) sb.Append(',');
//            Visit(node.Arguments[i]);
//        }
//        sb.Append(')');
//        return node;
//    }

//    protected override Expression VisitConditional(ConditionalExpression node)
//    {
//        sb.Append("CD(");
//        Visit(node.Test);
//        sb.Append('?');
//        Visit(node.IfTrue);
//        sb.Append(':');
//        Visit(node.IfFalse);
//        sb.Append(')');
//        return node;
//    }

//    private static void AppendTypeName(StringBuilder sb, string name)
//    {
//        const string ANONYMOUS_PREFIX = "<>f__AnonymousType";
//        var i = name.IndexOf(ANONYMOUS_PREFIX);
//        if (i > -1)
//        {
//            sb.Append('A');
//            var ccc = name.AsSpan().Slice(i + ANONYMOUS_PREFIX.Length);
//            for (int j = 0; j < ccc.Length; j++)
//            {
//                sb.Append(ccc[j]);
//            }
//        }
//        else
//        {
//            sb.Append(name);
//        }

//    }
//}


internal class ExpressionHasher : ExpressionVisitor, IResetable
{

    private const ulong FNVOffsetBasis = 0xCBF29CE484222325;
    private const ulong FNVPrime = 0x100000001B3;
    private ulong _hashCode = FNVOffsetBasis;
    private int _parameterIndex = 0;
    private readonly Dictionary<ParameterExpression, int> _parameterMap = new();
    private readonly Stack<HashScope> _scopes = new();

    // 缓存常用类型的元数据令牌
    private static readonly ConcurrentDictionary<Type, int> _typeTokenCache = new();
    private static readonly ConcurrentDictionary<MemberInfo, int> _memberTokenCache = new();

    public void Reset()
    {
        _hashCode = FNVOffsetBasis;
        _parameterIndex = 0;
        _parameterMap.Clear();
        _scopes.Clear();
    }

    public static ExpressionHasher Default => ExpressionVisitorPool<ExpressionHasher>.Rent();

    /// <summary>
    /// 计算表达式的64位哈希值（低碰撞率）
    /// </summary>
    public ulong ComputeHash64(Expression? node, bool includeParameterNames = false)
    {
        try
        {
            if (node is null) return 0;

            if (includeParameterNames)
            {
                _scopes.Push(new HashScope(ref _parameterIndex, _parameterMap));
            }

            Visit(node);

            if (includeParameterNames)
            {
                _scopes.Pop();
            }

            return _hashCode;
        }
        finally
        {
            ExpressionVisitorPool<ExpressionHasher>.Return(this);
        }
    }

    /// <summary>
    /// 计算32位哈希（兼容旧代码）
    /// </summary>
    public int ComputeHash32(Expression? node, bool includeParameterNames = false)
    {
        var hash64 = ComputeHash64(node, includeParameterNames);
        return (int)(hash64 ^ (hash64 >> 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddToHash(ulong value)
    {
        unchecked
        {
            _hashCode ^= value;
            _hashCode *= FNVPrime;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddToHash(int value) => AddToHash((ulong)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddToHash(long value) => AddToHash((ulong)value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddToHash(bool value) => AddToHash(value ? 1UL : 0UL);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddToHash(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            AddToHash(0);
            return;
        }

        // 使用快速的字符串哈希
        unchecked
        {
            ulong hash = 5381;
            foreach (char c in value)
            {
                hash = ((hash << 5) + hash) ^ c;
            }
            AddToHash(hash);
        }
    }

    private void AddTypeInfo(Type type)
    {
        if (type == null)
        {
            AddToHash(0);
            return;
        }

        // 使用元数据令牌作为类型标识
        if (!_typeTokenCache.TryGetValue(type, out var token))
        {
            try
            {
                // 尝试获取元数据令牌
                token = type.MetadataToken;
            }
            catch
            {
                // 回退到类型名称哈希
                token = type.GetHashCode();
            }
            _typeTokenCache.TryAdd(type, token);
        }

        AddToHash(token);
    }

    private void AddMemberInfo(MemberInfo member)
    {
        if (member == null)
        {
            AddToHash(0);
            return;
        }

        if (!_memberTokenCache.TryGetValue(member, out var token))
        {
            try
            {
                token = member.MetadataToken;
            }
            catch
            {
                token = member.GetHashCode();
            }
            _memberTokenCache.TryAdd(member, token);
        }

        AddToHash(token);
    }

    public override Expression? Visit(Expression? node)
    {
        if (node == null) return null;

        // 添加节点类型
        AddToHash((int)node.NodeType);

        // 添加类型信息
        AddTypeInfo(node.Type);

        return base.Visit(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        AddToHash(node.IsLifted);
        AddToHash(node.IsLiftedToNull);

        if (node.Method != null)
        {
            AddMemberInfo(node.Method);
        }

        base.VisitBinary(node);
        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.Method != null)
        {
            AddMemberInfo(node.Method);
        }

        base.VisitUnary(node);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value == null)
        {
            AddToHash(0xBADC0DE5);
        }
        else
        {
            var type = node.Value.GetType();
            AddTypeInfo(type);

            // 为常见值类型提供优化处理
            switch (node.Value)
            {
                case string str:
                    AddToHash(str);
                    break;
                case int i:
                    AddToHash((ulong)i);
                    break;
                case long l:
                    AddToHash((ulong)l);
                    break;
                case bool b:
                    AddToHash(b);
                    break;
                case decimal d:
                    var bits = decimal.GetBits(d);
                    foreach (var bit in bits)
                    {
                        AddToHash((ulong)bit);
                    }
                    break;
                case double dbl:
                    AddToHash(DoubleToUInt64Bits(dbl));
                    break;
                case float f:
                    AddToHash(SingleToInt64Bits(f));
                    break;
                case IQueryable:
                    // 忽略IQueryable的具体实例
                    AddToHash(0x5155455259); // "QUERY"的ASCII码
                    break;
                case Enum e:
                    AddToHash(Convert.ToUInt64(e));
                    break;
                case Guid guid:
                    var guidBytes = guid.ToByteArray();
                    AddToHash(BitConverter.ToUInt64(guidBytes, 0));
                    AddToHash(BitConverter.ToUInt64(guidBytes, 8));
                    break;
                case DateTime dateTime:
                    AddToHash(dateTime.Ticks);
                    break;
                case DateTimeOffset dateTimeOffset:
                    AddToHash(dateTimeOffset.Ticks);
                    AddToHash(dateTimeOffset.Offset.Ticks);
                    break;
                case TimeSpan timeSpan:
                    AddToHash(timeSpan.Ticks);
                    break;
                case byte b:
                    AddToHash(b);
                    break;
                case sbyte sb:
                    AddToHash((ulong)sb);
                    break;
                case short s:
                    AddToHash((ulong)s);
                    break;
                case ushort us:
                    AddToHash(us);
                    break;
                case uint ui:
                    AddToHash(ui);
                    break;
                case char c:
                    AddToHash(c);
                    break;
                default:
                    // 使用值的哈希码
                    AddToHash((ulong)node.Value.GetHashCode());
                    break;
            }
        }

        return node;

        static ulong DoubleToUInt64Bits(double value)
        {
#if NET462
            return (ulong)BitConverter.DoubleToInt64Bits(value);
#else
            return BitConverter.DoubleToUInt64Bits(value);
#endif
        }

        static long SingleToInt64Bits(float value)
        {
#if NET462
            return BitConverter.DoubleToInt64Bits(value);
#else
            return (long)BitConverter.SingleToUInt32Bits(value);
#endif
        }
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (_scopes.Count > 0) // 包含参数名模式
        {
            if (!_parameterMap.TryGetValue(node, out var index))
            {
                index = ++_parameterIndex;
                _parameterMap[node] = index;
            }

            AddToHash(index);
            AddTypeInfo(node.Type);

            if (!string.IsNullOrEmpty(node.Name))
            {
                AddToHash(node.Name);
            }
        }
        else // 忽略参数名模式
        {
            AddTypeInfo(node.Type);
        }

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        AddMemberInfo(node.Member);
        base.VisitMember(node);
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        AddMemberInfo(node.Method);

        if (node.Method.IsGenericMethod)
        {
            foreach (var arg in node.Method.GetGenericArguments())
            {
                AddTypeInfo(arg);
            }
        }

        base.VisitMethodCall(node);
        return node;
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        AddTypeInfo(node.Type);

        if (node.NewExpression.Constructor != null)
        {
            AddMemberInfo(node.NewExpression.Constructor);
        }

        foreach (var binding in node.Bindings)
        {
            AddMemberInfo(binding.Member);
            AddToHash((int)binding.BindingType);
        }

        base.VisitMemberInit(node);
        return node;
    }

    protected override Expression VisitNew(NewExpression node)
    {
        AddTypeInfo(node.Type);

        if (node.Constructor != null)
        {
            AddMemberInfo(node.Constructor);
        }

        if (node.Members != null)
        {
            foreach (var member in node.Members)
            {
                AddMemberInfo(member);
            }
        }

        base.VisitNew(node);
        return node;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        // 开始新的参数作用域
        var scope = new HashScope(ref _parameterIndex, _parameterMap);
        _scopes.Push(scope);

        // 处理参数
        foreach (var param in node.Parameters)
        {
            VisitParameter(param);
        }

        // 处理主体
        Visit(node.Body);

        _scopes.Pop();
        return node;
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        AddTypeInfo(node.TypeOperand);
        base.VisitTypeBinary(node);
        return node;
    }

    // 辅助结构体，用于管理参数作用域
    private struct HashScope
    {
        public int SavedIndex;
        public Dictionary<ParameterExpression, int> SavedMap;

        public HashScope(ref int currentIndex, Dictionary<ParameterExpression, int> currentMap)
        {
            SavedIndex = currentIndex;
            SavedMap = new Dictionary<ParameterExpression, int>(currentMap);
            currentIndex = 0;
            currentMap.Clear();
        }
    }

}