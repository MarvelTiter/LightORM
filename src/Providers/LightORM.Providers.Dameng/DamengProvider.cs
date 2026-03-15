using Dm;
using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.Dameng;

public sealed class DamengProvider : BaseDatabaseProvider
{
    public static DamengProvider Create(DataBaseOption option) => new(option);
    public static DamengProvider Create(Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    private DamengProvider(DataBaseOption option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new DamengTableHandler(option.GenerateOption);
        var sqlMethodResolver = new DamengMethodResolver(option.GenerateOption);
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        CustomDatabase = new CustomDameng(sqlMethodResolver, option.GenerateOption);
        CustomDatabase.AddKeyWord(option.Keyworks);
        CustomDatabase.UseIdentifierQuote = option.IsUseIdentifierQuote;
        DbProviderFactory = option.NewFactory ?? DmClientFactory.Instance;
    }

    public override DbBaseType DbBaseType => DbBaseType.Dameng;

    public override ICustomDatabase CustomDatabase { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; }

    public override DbProviderFactory DbProviderFactory { get; }

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
