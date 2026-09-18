using LightORM.Interfaces;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using LightORM.Implements;
using LightORM.Models;

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

    private MySqlProvider(DataBaseOption<MySqlTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var master = option.MasterConnectionString!;
        var match = Regex.Match(master, @"(?<=Database\=)([A-Z|a-z|_]+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            throw new Exception("未能在连接字符串中发现目标数据库!");
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
