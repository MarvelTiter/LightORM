using LightORM.Implements;
using LightORM.Models;
#if NET462_OR_GREATER
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace LightORM.Providers.SqlServer;

/// <summary>
/// SqlServer 方言随版本变化的能力位。
/// <para>
/// <b>只登记"不同版本有不同写法"的能力</b>——同一个 SQL 片段要能按版本生成两套实现，能力位才有意义。
/// 只有一种写法的能力不进这里（例如 JSON 的 <c>JSON_VALUE</c>/<c>JSON_MODIFY</c> 仅 2016+ 存在，
/// 低版本没有替代写法，硬判版本反而要多维护一条分支，直接交给数据库报错即可）。
/// </para>
/// <para>
/// 已登记的两个能力都是"新写法在老版本报错、旧写法在新版本仍可跑"，所以<b>版本未知时取
/// <see cref="SqlServerFeatures.Full"/></b>——保守退化会让现代库白白用上旧语法（如 ROW_NUMBER 分页）。
/// </para>
/// </summary>
[Flags]
public enum SqlServerFeatures
{
    None = 0,

    /// <summary>11.0 (2012)+：分页可用 <c>OFFSET ... FETCH NEXT</c>，否则退回 ROW_NUMBER 包裹。</summary>
    OffsetFetch = 1 << 0,

    /// <summary>14.0 (2017)+：原生 <c>TRIM()</c>，否则 <c>LTRIM(RTRIM())</c>。</summary>
    TrimFunction = 1 << 1,

    /// <summary>完整功能位集合。</summary>
    Full = OffsetFetch | TrimFunction,
}

/// <summary>
/// SqlServer 能力档案：把核心探测到的版本号翻译成 SqlServer 自己的能力位。
/// </summary>
public sealed class SqlServerCapabilities : DbCapabilities<SqlServerCapabilities, SqlServerFeatures>
{
    protected override string ProbeTarget => DbBaseType.SqlServer.Name;

    /// <summary>
    /// 用强类型 <see cref="SqlConnectionStringBuilder.ConnectTimeout"/> 压探测超时
    /// （SQL Server 驱动默认连接超时 15 秒，探测不能等这么久）。
    /// </summary>
    protected override string? PrepareProbeConnectString(string connectionString, int timeoutSeconds)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = timeoutSeconds,
            };
            return builder.ConnectionString;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 版本 → 能力位。版本不可用时按<b>完整功能</b>。
    /// </summary>
    public override SqlServerFeatures MapFeatures(Version? version)
    {
        if (version is null)
        {
            return SqlServerFeatures.Full;
        }

        var features = SqlServerFeatures.None;
        if (version.Major >= 11)
        {
            features |= SqlServerFeatures.OffsetFetch;      // 2012
        }
        if (version.Major >= 14)
        {
            features |= SqlServerFeatures.TrimFunction;     // 2017
        }
        return features;
    }
}
