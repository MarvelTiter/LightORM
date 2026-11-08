using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;
using LightORM.Options;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : BaseDatabaseProvider
{
    public static OracleProvider Create(DataBaseOption<OracleTableOptions> option) => new(option);
    public static OracleProvider Create(Action<DataBaseOption<OracleTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<OracleTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }
    /// <summary>
    /// 从配置文件创建（由 <c>UseOracle()</c> 空参重载登记）。
    /// <para>
    /// <see cref="DatabaseDefinition.DataSource"/> 优先（TNS 别名 / 完整描述符 / EZ Connect）；
    /// 否则按 <c>ServiceName</c> / <c>Sid</c> + Server(+Port) 组装；只给 Server 时当作 TNS 别名。
    /// </para>
    /// </summary>
    public static OracleProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        return Create(database.ToDbOption<OracleTableOptions>(BuildConnectionString(database.Key, definition)));
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new OracleConnectionStringBuilder()
                : new OracleConnectionStringBuilder(connectionString),
            (builder, def) =>
            {
                if (ResolveDataSource(key, def) is { Length: > 0 } dataSource)
                {
                    builder.DataSource = dataSource;
                }

                if (def.UserId is { Length: > 0 } userId) builder.UserID = userId;
                if (def.Password is { } password) builder.Password = password;
            });
    }

    /// <summary>
    /// Oracle 的 DataSource 组装：显式 → ServiceName → Sid → 裸 Server（当作 TNS 别名）。
    /// </summary>
    private static string? ResolveDataSource(string? key, DatabaseDefinition definition)
    {
        if (definition.DataSource is { Length: > 0 } explicitDataSource)
        {
            return explicitDataSource;
        }

        if (definition.ServiceName is { Length: > 0 } && definition.Sid is { Length: > 0 })
        {
            throw new LightOrmException($"数据库 '{key}'：ServiceName 与 Sid 只能二选一。");
        }

        var hasServer = definition.Server is { Length: > 0 };

        if (definition.ServiceName is { Length: > 0 } serviceName)
        {
            if (!hasServer)
            {
                throw new LightOrmException($"数据库 '{key}'：配置了 ServiceName 就必须同时配置 Server。");
            }

            // EZ Connect：host:port/service
            return $"{definition.Server}:{definition.Port ?? 1521}/{serviceName}";
        }

        if (definition.Sid is { Length: > 0 } sid)
        {
            if (!hasServer)
            {
                throw new LightOrmException($"数据库 '{key}'：配置了 Sid 就必须同时配置 Server。");
            }

            // SID 不能用 EZ Connect 表达：host:port/xxx 里的 xxx 在 Oracle 语法里是 service_name，
            // 只有单实例且 service_name 恰好等于 SID 时才"看起来能用"，RAC（SID=ORCL1，service=ORCL）必失败。
            return $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={definition.Server})(PORT={definition.Port ?? 1521}))(CONNECT_DATA=(SID={sid})))";
        }

        if (definition.Server is { Length: > 0 } tnsAlias)
        {
            // 裸 Server 当 TNS 别名（整串精确匹配 tnsnames.ora）。
            // 但一旦给了 Port，就不再是别名，而 host:port 缺少 service_name 也无法定位到库，
            // 与其拼出 "RACE:1521" 这种既不解析别名、又连不到具体服务的串，不如直接报错。
            if (definition.Port is { } port)
            {
                throw new LightOrmException($"数据库 '{key}'：配置了 Server 和 Port 时，必须再配置 ServiceName 或 Sid（EZ Connect 的 host:port 后面必须跟 service_name，不能只给主机端口）。只连 TNS 别名请去掉 Port。");
            }

            return tnsAlias;
        }

        return null;
    }

    private OracleProvider(DataBaseOption<OracleTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var generate = option.GenerateOption;
        var sqlMethodResolver = new OracleMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DbProviderFactory = option.NewFactory ?? OracleClientFactory.Instance;
        // 版本探测在构造期发起（后台执行），使首个 SQL 构建时档案已就绪；
        // 已用 TableOptions.SpecificVersion 指定版本、或 DetectVersion = false 时跳过探测（退到保守基线）。
        Capabilities = OracleCapabilities.Start(DbProviderFactory, MasterConnectionString, generate.SpecificVersion, generate.DetectVersion);
        // DbHandler 的 DDL 分支依赖能力档案（自增列走 IDENTITY 还是序列 + 触发器），故必须后建。
        DbHandler = new OracleTableHandler(generate, Capabilities);
        DatabaseAdapter = new CustomOracleAdapter(sqlMethodResolver, generate, Capabilities);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
    }

    /// <summary>数据库版本能力档案（同 <see cref="DatabaseAdapter"/> 上的那份，便于直接取用）。</summary>
    public OracleCapabilities Capabilities { get; }

    public override DbBaseType DbBaseType => DbBaseType.Oracle;

    public override IDatabaseAdapter DatabaseAdapter { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; }

    public override DbProviderFactory DbProviderFactory { get; }

    public override int BulkCopy(DataTable dataTable)
    {
        if (dataTable == null || dataTable.Columns.Count == 0 || dataTable.Rows.Count == 0)
        {
            throw new ArgumentException($"{nameof(dataTable)}为Null或零列零行.");
        }
        using var conn = (OracleConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();
        using var bulkcopy = new OracleBulkCopy(conn, OracleBulkCopyOptions.UseInternalTransaction)
        {
            DestinationTableName = DatabaseAdapter.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            bulkcopy.ColumnMappings.Add(new OracleBulkCopyColumnMapping(item.ColumnName, DatabaseAdapter.Emphasis.Insert(1, item.ColumnName)));
        }
        try
        {
            bulkcopy.WriteToServer(dataTable);
        }
        catch
        {
            throw;
        }
        finally
        {
            bulkcopy?.Close();
            conn?.Close();
        }

        return dataTable.Rows.Count;

    }

}
