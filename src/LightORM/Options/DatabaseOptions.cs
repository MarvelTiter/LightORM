namespace LightORM.Options;

/// <summary>
/// 配置文件里的一个数据库实例。
/// <para>
/// 与代码路径的 <c>DataBaseOption&lt;T&gt;</c> 是<b>两套模型</b>：
/// 这里只放"能跨方言表达"的字段，方言私有配置走 <see cref="DatabaseDefinition.Settings"/>，
/// 由各自 provider 自己解释。不要试图在这里穷举所有 provider 的选项。
/// </para>
/// </summary>
public class DatabaseOptions
{
    /// <summary>实例名，同时是 <c>DatabaseProviders</c> 的 key；为空时落到 <c>MainDb</c>。</summary>
    public string? Key { get; set; }

    /// <summary>连接定义。缺失时该条目被忽略（并给出告警）。</summary>
    public DatabaseDefinition? Option { get; set; }
}

/// <summary>
/// 通用连接定义。<see cref="ConnectionString"/> 与"分散字段"同时存在时，
/// 以连接串为底、分散字段逐项覆盖。
/// </summary>
public class DatabaseDefinition
{
    /// <summary>方言标识，必须与 <see cref="DbBaseType.Name"/> 一致（大小写不敏感）。</summary>
    public string? Type { get; set; }

    /// <summary>完整连接串。可缺省，此时由 <see cref="Server"/> / <see cref="Port"/> 等字段拼装。</summary>
    public string? ConnectionString { get; set; }

    public string? UserId { get; set; }
    public string? Password { get; set; }
    public string? Server { get; set; }
    public int? Port { get; set; }
    public string? Database { get; set; }

    /// <summary>
    /// 完整的数据源标识，优先级高于 <see cref="Server"/>：
    /// Oracle 的 TNS 别名 / EZ Connect 串、Sqlite 的库文件路径、SqlServer 的 <c>Data Source</c>。
    /// </summary>
    public string? DataSource { get; set; }

    /// <summary>Oracle 服务名。与 <see cref="Server"/>（+ <see cref="Port"/>）组合成 <c>host:port/service</c>。</summary>
    public string? ServiceName { get; set; }

    /// <summary>Oracle SID。与 <see cref="Server"/>（+ <see cref="Port"/>）组合成完整连接描述符；与 <see cref="ServiceName"/> 二选一。</summary>
    public string? Sid { get; set; }

    /// <summary>从库连接串（读写分离）。</summary>
    public string[]? SlaveConnectionStrings { get; set; }

    /// <summary>是否引用标识符（<c>"Name"</c>）。null 表示由方言默认值决定。</summary>
    public bool? IsUseIdentifierQuote { get; set; }

    /// <summary>显式指定服务端版本（如 <c>"8.0"</c>）。有值时<b>不发起版本探测</b>；ToSql 纯生成场景应填。</summary>
    public string? SpecificVersion { get; set; }

    /// <summary>是否允许构造期后台探测版本。null 表示用基类默认（true）。</summary>
    public bool? DetectVersion { get; set; }

    /// <summary>默认字符串长度（字符数）。null 表示用基类默认。</summary>
    public int? DefaultStringLength { get; set; }

    /// <summary>
    /// 方言私有配置。键名由各 provider 自己定义，例如达梦的 <c>TableSpace</c> / <c>GenerateUserId</c>。
    /// </summary>
    public Dictionary<string, string>? Settings { get; set; }

    /// <summary>取一个方言私有配置值。</summary>
    public string? GetSetting(string name)
        => Settings is not null && Settings.TryGetValue(name, out var value) ? value : null;
}

/// <summary>配置文件的根对象。</summary>
public class DatabaseList
{
    public List<DatabaseOptions> LightOrmDatabases { get; set; } = [];
}
