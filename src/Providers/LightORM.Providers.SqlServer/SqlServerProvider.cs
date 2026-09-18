using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;
#if NET462_OR_GREATER
using System.Data.SqlClient;
#else
using  Microsoft.Data.SqlClient;
#endif
namespace LightORM.Providers.SqlServer;

public sealed class SqlServerProvider : BaseDatabaseProvider
{
    public static SqlServerProvider Create(DataBaseOption<SqlServerTableOptions> option) => new(option);

    public static SqlServerProvider Create(Action<DataBaseOption<SqlServerTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<SqlServerTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    private SqlServerProvider(DataBaseOption<SqlServerTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var generate = option.GenerateOption;
        var factory = option.NewFactory ?? SqlClientFactory.Instance;
        // 能力档案先建：handler / methodResolver / adapter 都要读它。
        // 探测在构造期发起（不阻塞），结果在 SQL 生成期才被读取。
        DbProviderFactory = factory;
        Capabilities = SqlServerCapabilities.Start(factory, MasterConnectionString, generate.SpecificVersion, generate.DetectVersion);
        DbHandler = new SqlServerTableHandler(generate);
        var sqlMethodResolver = new SqlServerMethodResolver(Capabilities);
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomSqlServerAdapter(Capabilities, sqlMethodResolver, generate);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
    }

    public override DbBaseType DbBaseType => DbBaseType.SqlServer;

    /// <summary>
    /// 版本能力档案：由探测结果 / <see cref="TableOptions.SpecificVersion"/> 推导；
    /// 未启用探测、探测失败或版本未知时按<b>完整功能</b>。
    /// </summary>
    public SqlServerCapabilities Capabilities { get; }

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
