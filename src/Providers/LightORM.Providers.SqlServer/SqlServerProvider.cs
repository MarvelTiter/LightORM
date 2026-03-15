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
        CustomDatabase = new CustomSqlServer(version, sqlMethodResolver, option.GenerateOption);
        CustomDatabase.AddKeyWord(option.Keyworks);
        CustomDatabase.UseIdentifierQuote = option.IsUseIdentifierQuote;
        DbProviderFactory = option.NewFactory ?? SqlClientFactory.Instance;
    }

    public override DbBaseType DbBaseType => DbBaseType.SqlServer;

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
        var conn = (SqlConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();

        using var trans = conn.BeginTransaction();
        var sqlBulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, trans)
        {
            DestinationTableName = CustomDatabase.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(item.ColumnName, CustomDatabase.Emphasis.Insert(1, item.ColumnName)));
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
