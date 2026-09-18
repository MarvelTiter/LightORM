using LightORM.Implements;
using LightORM.Models;
using Oracle.ManagedDataAccess.Client;

namespace LightORM.Providers.Oracle;

/// <summary>
/// Oracle 方言随版本变化的能力位。
/// <para>
/// <b>只登记"不同版本有不同写法"的能力</b>——同一个 SQL/DDL 片段要能按版本生成两套实现，能力位才有意义。
/// 只有一种写法的能力不进这里：低版本没有替代写法，硬判版本多一条分支，直接交给数据库报错即可。
/// </para>
/// <para>
/// Oracle 取<b>保守</b>基线（版本未知 → <see cref="OracleFeatures.None"/>），与 SqlServer 的乐观基线相反：
/// Oracle 的能力位是"新写法在旧库<b>直接失败</b>"（19c 建 <c>JSON</c> 列报 ORA-00902、`GENERATED AS IDENTITY`
/// 在 12.1 前是语法错），所以退化到旧写法才是安全解；SqlServer 的能力位则是"新写法在旧库报错、
/// 旧写法在新库仍可跑"，故它选择乐观。
/// </para>
/// </summary>
[Flags]
public enum OracleFeatures
{
    None = 0,

    /// <summary>
    /// 12.1+：自增列可用 <c>GENERATED ALWAYS AS IDENTITY</c>，否则用序列 + 触发器（两套 DDL 实现）。
    /// </summary>
    IdentityColumn = 1 << 0,

    /// <summary>
    /// 21c+：原生 <c>JSON</c> 列类型、<c>JSON()</c> 构造器可用（≤19c 建表即 ORA-00902 invalid datatype）。
    /// 关掉时：列类型退化 <c>CLOB</c>，路径更新值改由 SQL/JSON 的 <c>FORMAT JSON</c> 子句承载（两套写法）。
    /// </summary>
    JsonNativeType = 1 << 1,
}

/// <summary>
/// Oracle 能力档案：把核心探测到的版本号翻译成 Oracle 自己的能力位。
/// </summary>
public sealed class OracleCapabilities : DbCapabilities<OracleCapabilities, OracleFeatures>
{
    protected override string ProbeTarget => DbBaseType.Oracle.Name;

    /// <summary>
    /// 用强类型 <see cref="OracleConnectionStringBuilder.ConnectionTimeout"/> 压探测超时。
    /// <para>
    /// 不用"超时键名"字符串写法：Oracle 对 <c>"Connect Timeout"</c>/<c>"Timeout"</c> 报
    /// ORA-50008 invalid connection string attribute，只有 <c>"Connection Timeout"</c> 被接受——
    /// 这类差异写错只能在运行期发现，而强类型属性由编译器保证。
    /// </para>
    /// </summary>
    protected override string? PrepareProbeConnectString(string connectionString, int timeoutSeconds)
    {
        try
        {
            var builder = new OracleConnectionStringBuilder(connectionString)
            {
                ConnectionTimeout = timeoutSeconds,
            };
            return builder.ConnectionString;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 版本 → 能力位。版本不可用时返回保守基线（各能力位都退到"全版本可用"的写法）：
    /// <c>None</c> = 不自增列（序列 + 触发器）+ CLOB 文本列，与引入探测前的手写默认值一致。
    /// </summary>
    public override OracleFeatures MapFeatures(Version? version)
    {
        if (version is null)
        {
            return OracleFeatures.None;
        }

        var features = OracleFeatures.None;
        // 12.1 起支持 IDENTITY；比分量而非构造 Version 实例。注意不能用 "Major >= 12 && Minor >= 1"：
        // 它会漏掉 minor 为 0 的 12.1+ 版本（如 19.0）。
        if (version.Major >= 13 || (version.Major == 12 && version.Minor >= 1))
        {
            features |= OracleFeatures.IdentityColumn;
        }
        if (version.Major >= 21)
        {
            features |= OracleFeatures.JsonNativeType;
        }
        return features;
    }
}
