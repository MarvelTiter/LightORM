using System.Reflection;

namespace LightORM.Utils;

/// <summary>
/// json 列参数绑定的公共辅助。
/// <para>
/// 框架(Builder/批量参数收集)不再对 json 列参数做预先序列化, 而是把<b>原始 CLR 值</b>连同列元数据
/// 交给数据库方言的 <see cref="IDatabaseParameterBinder"/>。本类为各方言提供统一的序列化, 以及
/// "值在 JSON 中是复合文档(对象/数组)还是标量"的判定 —— 方言据此决定 SQL 侧是否需要驱动特有的
/// 解析包装, 例如 SQLite 的 <c>JSON(@p)</c>、SqlServer 复合值的 <c>JSON_QUERY(@p)</c>。
/// </para>
/// </summary>
public static class JsonParameterHelper
{
    /// <summary>
    /// 按框架配置的 <see cref="ILightJsonHelper"/> 把值序列化为 JSON 文本。
    /// <para>
    /// 注意: 这是"JSON 值语义"的序列化, <c>string</c> 会得到带引号的 JSON 字符串(如 <c>"abc"</c>)，
    /// 与方言中 <c>SqlFn.JsonSet</c>(参数为用户原始值、不走此处)的语义不同。
    /// </para>
    /// </summary>
    public static string Serialize(object? value) => ExpressionSqlOptions.Instance.Value.GetJsonHandler().Serialize(value);

    /// <summary>
    /// 判断该 CLR 类型在 JSON 中表示的是"复合文档"(对象/数组), 而不是标量。
    /// 无法确定时(如 <c>object</c>/接口)按标量处理 —— 标量绑定(传原生值)是更安全的默认。
    /// </summary>
    public static bool IsCompositeJsonValue(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (t.IsPrimitive || t.IsEnum) return false;
        // object/接口等无法判定具体形状的静态类型按标量处理: 标量绑定(传原生值)是更安全的默认。
        if (t == typeof(object)) return false;
        if (t == typeof(string) || t == typeof(char) || t == typeof(decimal) || t == typeof(Guid)
            || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan)
            || t == typeof(byte[]) || t == typeof(Uri) || t == typeof(Version))
        {
            return false;
        }
        return true;
    }

    /// <inheritdoc cref="IsCompositeJsonValue(Type)"/>
    public static bool IsCompositeJsonValue(object? value) => value is not null && IsCompositeJsonValue(value.GetType());

    /// <summary>
    /// 判断 json 列的<b>路径引用</b>末端指向的是否为复合文档(对象/数组)。
    /// <para>
    /// 与 <see cref="IsCompositeJsonValue(Type)"/> 的区别: 这里针对的是"列 + 路径"(如
    /// <c>j.Data.NestJson</c> / <c>j.Arr[1]</c>)的取值, 供方言在<b>生成期</b>决定读取路径用哪个函数——
    /// SqlServer / Oracle 的 <c>JSON_VALUE</c> 只返回标量, 遇对象/数组返回 NULL, 必须改用
    /// <c>JSON_QUERY</c> 取 JSON 文档文本, 再由读取侧反序列化。
    /// </para>
    /// <para>
    /// 只用生成期信息(表达式结构 + 列元数据), 不看运行时值, 与表达式缓存兼容。
    /// 无法判定时(路径以索引结束且锚点不是数组/集合, 如 <c>Obj["City"]</c>)按标量处理, 保持既有语义。
    /// </para>
    /// </summary>
    public static bool IsCompositeJsonLeaf(JsonColumnContext context)
    {
        // Stack<MemberPathInfo> 的枚举顺序即 Pop 顺序(自栈顶向下), 最后一个元素既是最先被弹出、
        // 也是列名后 json 路径中最靠内的一级 —— 即"末级"。
        MemberPathInfo leaf = default;
        var hasLeaf = false;
        foreach (var mi in context.Members)
        {
            leaf = mi;
            hasLeaf = true;
        }
        if (!hasLeaf) return false;

        var type = leaf.Member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            // 路径以索引结束(如 Arr[1] / Lst[0])时, 末级没有成员信息, 锚点就是 json 列本身
            _ => context.Column.ColumnType
        };
        if (type is null) return false;
        // 有索引则取元素类型: 数组/List<T> 可判定, JsonNode 之类的索引器语义无法判定 → 按标量
        if (leaf.IndexValue.HasValue) type = GetElementType(type);
        return type is not null && IsCompositeJsonValue(type);
    }

    /// <summary>
    /// 取数组 / 单泛型参数集合(List&lt;T&gt; / IList&lt;T&gt; 等)的元素类型; 无法判定返回 null。
    /// </summary>
    private static Type? GetElementType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            if (args.Length == 1) return args[0];
        }
        return null;
    }
}
