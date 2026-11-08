using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;
using LightORM.Options;
#if NET462_OR_GREATER
using System.Data.SqlClient;
#else
using  Microsoft.Data.SqlClient;
#endif
namespace LightORM.Providers.SqlServer;

public sealed class SqlServerProvider : BaseDatabaseProvider
{
    public static SqlServerProvider Create(DataBaseOption<SqlServerTableOptions> option) => new(option);

    public static SqlServerProvider Create(Action<DataBaseOption<SqlServerTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<SqlServerTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    /// <summary>
    /// 从配置文件创建（由 <c>UseSqlServer()</c> 空参重载登记）。
    /// <para>
    /// <see cref="DatabaseDefinition.ConnectionString"/> 为底，分散字段逐项覆盖。
    /// 注意 <see cref="SqlConnectionStringBuilder"/> 没有 Port 属性：端口写成 <c>Data Source=host,port</c>。
    /// </para>
    /// </summary>
    public static SqlServerProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        return Create(database.ToDbOption<SqlServerTableOptions>(BuildConnectionString(database.Key, definition)));
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new SqlConnectionStringBuilder()
                : new SqlConnectionStringBuilder(connectionString),
            (builder, def) =>
            {
                if (def.DataSource is { Length: > 0 } dataSource)
                {
                    builder.DataSource = dataSource;
                }
                else if (def.Server is { Length: > 0 } server)
                {
                    if (def.Port is { } port)
                    {
                        // 已自带端口或命名实例时再追加会拼出 host,1433,port 这种非法值
                        if (server.IndexOf(',') >= 0 || server.IndexOf('\\') >= 0)
                        {
                            throw new LightOrmException(
                                $"数据库 '{key}'：Server '{server}' 已包含端口或实例名，不要再单独配置 Port。");
                        }

                        builder.DataSource = $"{server},{port}";
                    }
                    else
                    {
                        builder.DataSource = server;
                    }
                }
                else if (def.Port is not null)
                {
                    throw new LightOrmException(
                        $"数据库 '{key}'：SqlServer 的 Port 必须与 Server 一起配置，或直接写进连接串的 Data Source。");
                }

                if (def.UserId is { Length: > 0 } userId) builder.UserID = userId;
                if (def.Password is { } password) builder.Password = password;
                if (def.Database is { Length: > 0 } databaseName) builder.InitialCatalog = databaseName;
            });
    }

    private SqlServerProvider(DataBaseOption<SqlServerTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var generate = option.GenerateOption;
        var factory = option.NewFactory ?? SqlClientFactory.Instance;
        // 能力档案先建：handler / methodResolver / adapter 都要读它。
        // 探测在构造期发起（不阻塞），结果在 SQL 生成期才被读取。
        DbProviderFactory = factory;
        Capabilities = SqlServerCapabilities.Start(factory, MasterConnectionString, generate.SpecificVersion, generate.DetectVersion);
        DbHandler = new SqlServerTableHandler(generate);
        var sqlMethodResolver = new SqlServerMethodResolver(Capabilities);
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomSqlServerAdapter(Capabilities, sqlMethodResolver, generate);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
    }

    public override DbBaseType DbBaseType => DbBaseType.SqlServer;

    /// <summary>
    /// 版本能力档案：由探测结果 / <see cref="TableOptions.SpecificVersion"/> 推导；
    /// 未启用探测、探测失败或版本未知时按<b>完整功能</b>。
    /// </summary>
    public SqlServerCapabilities Capabilities { get; }

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
        var conn = (SqlConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();

        using var trans = conn.BeginTransaction();
        var sqlBulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, trans)
        {
            DestinationTableName = DatabaseAdapter.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(item.ColumnName, DatabaseAdapter.Emphasis.Insert(1, item.ColumnName)));
        }
        try
        {
            sqlBulkCopy.WriteToServer(dataTable);
            trans.Commit();
        }
        catch
        {
            trans?.Rollback();
            throw;
        }
        finally
        {
            sqlBulkCopy?.Close();
            trans?.Dispose();
            conn?.Close();
        }

        return dataTable.Rows.Count;
    }


}
