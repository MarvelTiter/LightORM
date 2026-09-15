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
}
