using Dm;
using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using LightORM.Implements;

namespace LightORM.Providers.Dameng;

public sealed class DamengProvider : BaseDatabaseProvider
{
    public static DamengProvider Create(string master, params string[] slaves) => new DamengProvider(master, slaves);
    private static readonly Lazy<IDatabaseTableHandler> lazyHandler = new(() => new DamengTableHandler());

    private DamengProvider(string master, params string[] slaves) : base(master, slaves)
    {

    }
    public override DbBaseType DbBaseType => DbBaseType.Dameng;
    public override ICustomDatabase CustomDatabase { get; } = CustomDameng.Instance;

    public override Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler => lazyHandler.Value;

    public override DbProviderFactory DbProviderFactory { get; set; } = DmClientFactory.Instance;

    public override int BulkCopy(DataTable dataTable)
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
            DestinationTableName = CustomDatabase.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            var col = dataTable.Columns[i];
            var mapping = new DmBulkCopyColumnMapping(i, CustomDatabase.Emphasis.Insert(1, col.ColumnName));
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
