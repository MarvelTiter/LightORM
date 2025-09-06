using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using LightORM.Implements;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : BaseDatabaseProvider
{
    public static OracleProvider Create(string master, params string[] slaves) => new OracleProvider(master, slaves);
    private static readonly Lazy<IDatabaseTableHandler> lazyHandler = new(() => new OracleTableHandler());
    private OracleProvider(string master, params string[] slaves):base(master,slaves)
    {
    }
    public override DbBaseType DbBaseType => DbBaseType.Oracle;

    public override ICustomDatabase CustomDatabase { get; } = CustomOracle.Instance;

    public override Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler => lazyHandler.Value;

    public override DbProviderFactory DbProviderFactory { get; set; } = OracleClientFactory.Instance;

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
