using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : IDatabaseProvider
{
    public static OracleProvider Create(string master, params string[] slaves) => new OracleProvider(master, slaves);
    private OracleProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }
    public DbBaseType DbBaseType => DbBaseType.Oracle;
    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomOracle.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new OracleTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; } = OracleClientFactory.Instance;

    public int BulkCopy(DataTable dataTable)
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
            DestinationTableName = CustomDatabase.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            bulkcopy.ColumnMappings.Add(new OracleBulkCopyColumnMapping(item.ColumnName, CustomDatabase.Emphasis.Insert(1, item.ColumnName)));
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
