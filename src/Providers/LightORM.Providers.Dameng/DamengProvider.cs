using Dm;
using LightORM.Interfaces;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.Dameng;

public sealed class DamengProvider : IDatabaseProvider
{
    public static DamengProvider Create(string master, params string[] slaves) => new DamengProvider(master, slaves);

    private DamengProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }
    public DbBaseType DbBaseType => DbBaseType.MySql;

    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomDameng.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new DamengTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; internal set; } = DmClientFactory.Instance;

    public int BulkCopy(DataTable dataTable)
    {
        if (dataTable == null || dataTable.Columns.Count == 0 || dataTable.Rows.Count == 0)
        {
            throw new ArgumentException($"{nameof(dataTable)}为Null或零列零行.");
        }
        using var conn = (DmConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();

        using var transcation = (DmTransaction)conn.BeginTransaction();
        var bulkCopy = new DmBulkCopy(conn, DmBulkCopyOptions.Default, transcation)
        {
            DestinationTableName = dataTable.TableName,
            BulkCopyTimeout = 120
        };

        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            var col = dataTable.Columns[i];
            var mapping = new DmBulkCopyColumnMapping(i, col.ColumnName);
            bulkCopy.ColumnMappings.Add(mapping);
        }
        int effectedRows = 0;
        try
        {
            bulkCopy.WriteToServer(dataTable);
            transcation.Commit();
        }
        catch
        {
            transcation.Rollback();
            effectedRows = 0;
            throw;
        }
        finally
        {
            transcation.Dispose();
            conn.Close();
        }
        return effectedRows;
    }

}
