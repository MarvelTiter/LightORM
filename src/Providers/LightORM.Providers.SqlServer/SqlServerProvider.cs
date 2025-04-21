using LightORM;
using LightORM.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.SqlServer;

public enum SqlServerVersion
{
    V1,
    Over2012,
    Over2017,
}

public sealed class SqlServerProvider : IDatabaseProvider
{
    public static SqlServerProvider Create(SqlServerVersion version, string master, params string[] slaves)
        => new SqlServerProvider(version, master, slaves);
    public static SqlServerProvider Create(ICustomDatabase customDatabase, string master, params string[] slaves)
        => new SqlServerProvider(customDatabase, master, slaves);
    public SqlServerProvider(SqlServerVersion version
        , string master
        , params string[] slaves)
    {
        CustomDatabase = new CustomSqlServer(version);
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }
    public SqlServerProvider(ICustomDatabase customDatabase
        , string master
        , params string[] slaves)
    {
        CustomDatabase = customDatabase;
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }
    public DbBaseType DbBaseType => DbBaseType.SqlServer;
    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; }

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new SqlServerTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; internal set; } = SqlClientFactory.Instance;
    
    public int BulkCopy(DataTable dataTable)
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
            DestinationTableName = dataTable.TableName,
            BulkCopyTimeout = 120
        };

        foreach (DataColumn item in dataTable.Columns)
        {
            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(item.ColumnName, item.ColumnName));
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
