using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : BaseDatabaseProvider
{
    public static OracleProvider Create(DataBaseOption option) => new(option);
    public static OracleProvider Create(Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }
    private OracleProvider(DataBaseOption option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new OracleTableHandler(option.GenerateOption);
        var sqlMethodResolver = new OracleMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomOracleAdapter(sqlMethodResolver, option.GenerateOption);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote;
        DbProviderFactory = option.NewFactory ?? OracleClientFactory.Instance;
    }
    public override DbBaseType DbBaseType => DbBaseType.Oracle;

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
        using var conn = (OracleConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();
        using var bulkcopy = new OracleBulkCopy(conn, OracleBulkCopyOptions.UseInternalTransaction)
        {
            DestinationTableName = DatabaseAdapter.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            bulkcopy.ColumnMappings.Add(new OracleBulkCopyColumnMapping(item.ColumnName, DatabaseAdapter.Emphasis.Insert(1, item.ColumnName)));
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
