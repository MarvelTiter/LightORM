using LightORM;
using LightORM.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using LightORM.Implements;

namespace LightORM.Providers.SqlServer;

public enum SqlServerVersion
{
    V1,
    Over2012,
    Over2017,
}

public sealed class SqlServerProvider : BaseDatabaseProvider
{
    public static SqlServerProvider Create(SqlServerVersion version, string master, params string[] slaves)
        => new(version, master, slaves);
    public static SqlServerProvider Create(ICustomDatabase customDatabase, string master, params string[] slaves)
        => new(customDatabase, master, slaves);
    private SqlServerProvider(SqlServerVersion version
        , string master
        , params string[] slaves): base(master, slaves)
    {
        CustomDatabase = new CustomSqlServer(version);
    }
    private SqlServerProvider(ICustomDatabase customDatabase
        , string master
        , params string[] slaves): base(master, slaves)
    {
        CustomDatabase = customDatabase;
    }
    public override DbBaseType DbBaseType => DbBaseType.SqlServer;

    public override ICustomDatabase CustomDatabase { get; }

    public override Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; } = new SqlServerTableHandler();
    
    public override DbProviderFactory DbProviderFactory { get; set; } = SqlClientFactory.Instance;

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
