using LightORM.Interfaces;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;
using LightORM.Options;

namespace LightORM.Providers.MySql;

public sealed class MySqlProvider : BaseDatabaseProvider
{
    public static MySqlProvider Create(DataBaseOption<MySqlTableOptions> option) => new (option);

    public static MySqlProvider Create(Action<DataBaseOption<MySqlTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<MySqlTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    /// <summary>
    /// 从配置文件创建（由 <c>UseMySql()</c> 空参重载登记）。
    /// <para>
    /// <see cref="DatabaseDefinition.ConnectionString"/> 为底，分散字段逐项覆盖；
    /// 没有连接串时用 Server / Port / UserId / Password / Database 拼装。
    /// </para>
    /// </summary>
    public static MySqlProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        return Create(database.ToDbOption<MySqlTableOptions>(BuildConnectionString(database.Key, definition)));
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new MySqlConnectionStringBuilder()
                : new MySqlConnectionStringBuilder(connectionString),
            (builder, def) =>
            {
                if (def.DataSource is { Length: > 0 } dataSource) builder.Server = dataSource;
                else if (def.Server is { Length: > 0 } server) builder.Server = server;

                if (def.Port is { } port) builder.Port = (uint)port;
                if (def.UserId is { Length: > 0 } userId) builder.UserID = userId;
                if (def.Password is { } password) builder.Password = password;
                if (def.Database is { Length: > 0 } databaseName) builder.Database = databaseName;
            });
    }

    private MySqlProvider(DataBaseOption<MySqlTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var master = option.MasterConnectionString!;
        // 用强类型 builder 取库名：原来的正则只认字母下划线，库名带数字/中文会误判为"未发现数据库"
        if (string.IsNullOrEmpty(new MySqlConnectionStringBuilder(master).Database))
        {
            throw new LightOrmException("连接字符串中缺少 Database（库名不能为空）。");
        }
        var generate = option.GenerateOption;
        var factory = option.NewFactory ?? MySqlConnectorFactory.Instance;
        // 版本探测在构造期发起（后台执行），使首个 SQL 构建时档案已就绪；
        // 已用 TableOptions.SpecificVersion 指定版本、或 DetectVersion = false 时跳过探测（退到保守基线）。
        Capabilities = MySqlCapabilities.Start(factory, master, generate.SpecificVersion, generate.DetectVersion);
        DbHandler = new MySqlTableHandler(master, generate);
        var sqlMethodResolver = new MySqlMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomMySqlAdapter(sqlMethodResolver, Capabilities);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
        DbProviderFactory = factory;
    }
    public override DbBaseType DbBaseType => DbBaseType.MySql;

    /// <summary>版本能力档案（探测 / 显式指定 / 未探测时的保守基线）。</summary>
    public MySqlCapabilities Capabilities { get; }

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
        if (!MasterConnectionString.Contains("allowloadlocalinfile", StringComparison.OrdinalIgnoreCase))
        {
            throw new LightOrmException("请在连接字符串配置AllowLoadLocalInfile=true;");
        }
        using var conn = (MySqlConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SET GLOBAL local_infile=1";
        cmd.ExecuteNonQuery();

        using var mySqlTransaction = conn.BeginTransaction();
        var mySqlBulkCopy = new MySqlBulkCopy(conn, mySqlTransaction)
        {
            DestinationTableName = dataTable.TableName,
            BulkCopyTimeout = 120
        };

        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            var col = dataTable.Columns[i];
            var mySqlBulkCopyColumnMapping = new MySqlBulkCopyColumnMapping(i, col.ColumnName, null);
            mySqlBulkCopy.ColumnMappings.Add(mySqlBulkCopyColumnMapping);
        }
        int effectedRows = 0;
        try
        {
            var mySqlBulkCopyResult = mySqlBulkCopy.WriteToServer(dataTable);
            effectedRows = mySqlBulkCopyResult.RowsInserted;
            mySqlTransaction.Commit();
        }
        catch
        {
            mySqlTransaction.Rollback();
            effectedRows = 0;
            throw;
        }
        finally
        {
            mySqlTransaction.Dispose();
            conn.Close();
        }
        return effectedRows;
    }

}
