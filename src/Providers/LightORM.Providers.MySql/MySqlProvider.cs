using LightORM.Interfaces;
using MySqlConnector;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.MySql;

public sealed class MySqlProvider : IDatabaseProvider
{
    public static MySqlProvider Create(string master, params string[] slaves) => new MySqlProvider(master, slaves);

    private MySqlProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }
    public DbBaseType DbBaseType => DbBaseType.MySql;

    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomMySql.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new MySqlTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; internal set; } = MySqlConnectorFactory.Instance;

    public int BulkCopy(DataTable dataTable)
    {
        if (dataTable == null || dataTable.Columns.Count == 0 || dataTable.Rows.Count == 0)
        {
            throw new ArgumentException($"{nameof(dataTable)}为Null或零列零行.");
        }
        if (!MasterConnectionString.ToLower().Contains("allowloadlocalinfile"))
        {
            LightOrmException.Throw("请在连接字符串配置AllowLoadLocalInfile=true;");
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
