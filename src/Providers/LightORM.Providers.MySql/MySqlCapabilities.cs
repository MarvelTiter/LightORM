using LightORM.Implements;
using LightORM.Models;
using MySqlConnector;

namespace LightORM.Providers.MySql;

/// <summary>
/// MySQL 方言随版本变化的能力位。
/// <para>
/// <b>只登记"不同版本有不同写法"的能力</b>——同一 SQL 片段要能按版本生成两套实现，能力位才有意义。
/// 只有一种写法的能力不进这里（例如函数索引 <c>((expr))</c> 仅 8.0.13+ 存在，5.7 没有替代写法，
/// 硬判版本只是多一条分支，交给数据库报错即可）。
/// </para>
/// <para>
/// MySQL 取<b>保守</b>基线（版本未知 → <see cref="MySqlFeatures.None"/>）：旧写法
/// （<c>GROUP BY ... WITH ROLLUP</c>）在 5.7 与 8.0 都能跑，而 8.0 的 <c>ROLLUP()</c> 语法在 5.7 直接报错，
/// 所以未知时选全版本可用的那个。
/// </para>
/// </summary>
[Flags]
public enum MySqlFeatures
{
    None = 0,

    /// <summary>
    /// 8.0+：<c>GROUP BY ROLLUP(...)</c> / <c>CUBE(...)</c> / <c>GROUPING SETS(...)</c>；
    /// 8.0 以下只有 <c>GROUP BY ... WITH ROLLUP</c> 一种写法（无 CUBE / GROUPING SETS）。
    /// </summary>
    GroupByRollup = 1 << 0,

    /// <summary>完整功能位集合。</summary>
    Full = GroupByRollup,
}

/// <summary>
/// MySQL 能力档案：把核心探测到的版本号翻译成 MySQL 自己的能力位。
/// </summary>
public sealed class MySqlCapabilities : DbCapabilities<MySqlCapabilities, MySqlFeatures>
{
    protected override string ProbeTarget => DbBaseType.MySql.Name;

    /// <summary>
    /// 用强类型 <see cref="MySqlConnectionStringBuilder.ConnectionTimeout"/> 压探测超时
    /// （注意该属性是 <c>uint</c>，与 <c>DefaultCommandTimeout</c> 是两个不同属性——按"超时键名"字符串试探
    /// 区分不了两者，这正是交给方言而非核心的原因）。
    /// </summary>
    protected override string? PrepareProbeConnectString(string connectionString, int timeoutSeconds)
    {
        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                ConnectionTimeout = (uint)timeoutSeconds,
            };
            return builder.ConnectionString;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 版本 → 能力位。版本不可用时返回保守基线。
    /// <para>
    /// 直接比版本分量（major = 8 即 MySQL 8.0 起），不构造 <c>Version</c> 实例比较。
    /// MariaDB 会伪装 "5.5.5-10.x-MariaDB" 前缀，按前导数字取到 major 5 → 自动落到保守基线
    /// （MariaDB 无 <c>ROLLUP()</c> 语法），与预期一致。
    /// </para>
    /// </summary>
    public override MySqlFeatures MapFeatures(Version? version)
    {
        if (version is null)
        {
            return MySqlFeatures.None;
        }

        var features = MySqlFeatures.None;
        if (version.Major >= 8)
        {
            features |= MySqlFeatures.GroupByRollup;    // 8.0
        }
        return features;
    }
}
