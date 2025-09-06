using LightORM.Interfaces;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using LightORM.Implements;

namespace LightORM.Providers.MySql;

public sealed class MySqlProvider : BaseDatabaseProvider
{
    public static MySqlProvider Create(string master, params string[] slaves) => new MySqlProvider(master, slaves);
    private readonly Lazy<IDatabaseTableHandler> lazyHandler;
    private MySqlProvider(string master, params string[] slaves):base(master, slaves)
    {
        var match = Regex.Match(master, @"(?<=Database\=)([A-Z|a-z|_]+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            throw new Exception("未能在连接字符串中发现目标数据库!");
        }
        lazyHandler = new(() => new MySqlTableHandler(match.Value));
    }
    public override DbBaseType DbBaseType => DbBaseType.MySql;

    public override ICustomDatabase CustomDatabase { get; } = CustomMySql.Instance;

    public override Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler => lazyHandler.Value;

    public override DbProviderFactory DbProviderFactory { get; set; } = MySqlConnectorFactory.Instance;

    public override int BulkCopy(DataTable dataTable)
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
