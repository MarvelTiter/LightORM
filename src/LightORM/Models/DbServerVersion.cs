namespace LightORM.Models;

/// <summary>
/// 数据库版本的来源，优先级：<see cref="Explicit"/> &gt; <see cref="Probed"/> &gt; <see cref="Assumed"/>。
/// </summary>
public enum CapabilitySource
{
    /// <summary>用户显式指定，最高优先级，不做探测。</summary>
    Explicit = 0,

    /// <summary>运行期连接数据库、读取服务端版本后得出。</summary>
    Probed = 1,

    /// <summary>探测失败或尚未完成，版本不可信。</summary>
    Assumed = 2,
}

/// <summary>
/// 一次数据库版本探测的结果。
/// <para>
/// 核心只负责"拿到版本串"这件事，<b>不解释版本号的含义</b>——哪个版本支持哪种写法，
/// 属于各方言自己的知识，放在 provider 里。
/// </para>
/// </summary>
public readonly struct DbServerVersion(string? raw, Version? parsed, CapabilitySource source)
{

    /// <summary>驱动报告的服务端版本原始串。各方言格式不一（SQLite "3.46.1"、PG "16.4 (Debian …)"）。</summary>
    public string? Raw { get; } = raw;

    /// <summary>从 <see cref="Raw"/> 前导数字段解析出的版本；解析不出时为 null。</summary>
    public Version? Parsed { get; } = parsed;

    public CapabilitySource Source { get; } = source;

    /// <summary>版本是否可用（可解析）。</summary>
    public bool IsKnown => Parsed is not null;

    public override string ToString()
        => $"{(Raw ?? Parsed?.ToString() ?? "unknown")} [{Source}]";
}
