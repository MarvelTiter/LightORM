using LightORM;
using LightORM.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.SqlServer;

public enum SqlServerVersion
{
    V1,
    Over2012,
    Over2017,
}

public sealed class SqlServerProvider : BaseDatabaseProvider
{
    public static SqlServerProvider Create(SqlServerVersion version, DataBaseOption option)
        => new(version, option);

    public static SqlServerProvider Create(SqlServerVersion version, Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(version, dbOption);
    }

    private SqlServerProvider(SqlServerVersion version
        , DataBaseOption option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new SqlServerTableHandler(option.GenerateOption);
        var sqlMethodResolver = new SqlServerMethodResolver(version);
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomSqlServerAdapter(version, sqlMethodResolver, option.GenerateOption);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote;
        DbProviderFactory = option.NewFactory ?? SqlClientFactory.Instance;
    }

    public override DbBaseType DbBaseType => DbBaseType.SqlServer;

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
        var conn = (SqlConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();

        using var trans = conn.BeginTransaction();
        var sqlBulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, trans)
        {
            DestinationTableName = DatabaseAdapter.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(item.ColumnName, DatabaseAdapter.Emphasis.Insert(1, item.ColumnName)));
        }
        try
        {
            sqlBulkCopy.WriteToServer(dataTable);
            trans.Commit();
        }
        catch
        {
            trans?.Rollback();
            throw;
        }
        finally
        {
            sqlBulkCopy?.Close();
            trans?.Dispose();
            conn?.Close();
        }

        return dataTable.Rows.Count;
    }


}
