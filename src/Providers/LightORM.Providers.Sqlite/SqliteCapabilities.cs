using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.Sqlite;

/// <summary>
/// SQLite 方言随版本变化的能力位。
/// <para>
/// <b>只登记"不同版本有不同写法"的能力</b>——SQLite 的 JSON 函数族正是这种：3.45.0 起才有
/// <c>jsonb_*</c>（二进制存储，<c>jsonb_extract</c> / <c>jsonb_set</c>）与 <c>JSONB</c> 列，之前只有
/// <c>json_*</c> 文本函数。旧版遇到 <c>jsonb_extract</c> 是直接报 <c>no such function</c>，
/// 所以这是"新写法在旧库直接失败"的能力。
/// </para>
/// <para>
/// 版本未知时取<b>完整功能</b>（<see cref="SqliteFeatures.Full"/>），与 SqlServer 同为乐观基线：
/// 框架其它地方（<c>UPSERT</c>、窗口函数）本来就假定现代 SQLite，退化反而会让
/// <c>DetectVersion=false</c> 的 ToSql 场景生成出与今天不同的 SQL。
/// </para>
/// </summary>
[Flags]
public enum SqliteFeatures
{
    None = 0,

    /// <summary>3.45.0+：<c>jsonb_*</c> 函数族与 <c>JSONB</c> 列可用。</summary>
    Jsonb = 1 << 0,

    /// <summary>完整功能位集合。</summary>
    Full = Jsonb,
}

/// <summary>
/// SQLite 能力档案：把探测到的版本号翻译成 SQLite 自己的能力位。
/// </summary>
public sealed class SqliteCapabilities : DbCapabilities<SqliteCapabilities, SqliteFeatures>
{
    protected override string ProbeTarget => DbBaseType.Sqlite.Name;

    /// <summary>
    /// SQLite 是本地文件，没有"连接超时"这个概念，连接串无需改写；
    /// 且 <see cref="OpenBeforeRead"/> 为 false（见下），探测根本不会去建连。
    /// </summary>
    protected override string? PrepareProbeConnectString(string connectionString, int timeoutSeconds) => null;

    /// <summary>
    /// SQLite 未 Open 即可读 <c>ServerVersion</c>（实测 3.46.1 正常返回），
    /// 而 Open 会在文件不存在时<b>建出一个空库文件</b>——纯 SQL 生成场景绝不能碰。
    /// </summary>
    protected override bool OpenBeforeRead => false;

    /// <summary>
    /// 版本 → 能力位。版本不可用时按<b>完整功能</b>（保持与引入探测前一致的 SQL 输出）。
    /// <para>直接比版本分量：3.45 起有 JSONB，不构造 <c>Version</c> 实例比较。</para>
    /// </summary>
    public override SqliteFeatures MapFeatures(Version? version)
    {
        if (version is null)
        {
            return SqliteFeatures.Full;
        }

        var features = SqliteFeatures.None;
        if (version.Major > 3 || (version.Major == 3 && version.Minor >= 45))
        {
            features |= SqliteFeatures.Jsonb;   // 3.45.0
        }
        return features;
    }

    /// <summary>
    /// 是否使用二进制 JSON 后端：<b>用户选了 <see cref="JSONBackend.Binary"/> 且服务端确实支持</b>。
    /// 老版本 SQLite（&lt; 3.45）上没有 <c>jsonb_*</c>，此时退回 <c>json_*</c> 文本函数，
    /// 让"选错后端"从运行期报错变成自动降级。
    /// </summary>
    public bool UseBinaryJson(JSONBackend backend)
        => backend == JSONBackend.Binary && Features.HasFlag(SqliteFeatures.Jsonb);
}
