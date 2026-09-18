using LightORM.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.Oracle;

public sealed class OracleProvider : BaseDatabaseProvider
{
    public static OracleProvider Create(DataBaseOption<OracleTableOptions> option) => new(option);
    public static OracleProvider Create(Action<DataBaseOption<OracleTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<OracleTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }
    private OracleProvider(DataBaseOption<OracleTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var generate = option.GenerateOption;
        var sqlMethodResolver = new OracleMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DbProviderFactory = option.NewFactory ?? OracleClientFactory.Instance;
        // 版本探测在构造期发起（后台执行），使首个 SQL 构建时档案已就绪；
        // 已用 TableOptions.SpecificVersion 指定版本、或 DetectVersion = false 时跳过探测（退到保守基线）。
        Capabilities = OracleCapabilities.Start(DbProviderFactory, MasterConnectionString, generate.SpecificVersion, generate.DetectVersion);
        // DbHandler 的 DDL 分支依赖能力档案（自增列走 IDENTITY 还是序列 + 触发器），故必须后建。
        DbHandler = new OracleTableHandler(generate, Capabilities);
        DatabaseAdapter = new CustomOracleAdapter(sqlMethodResolver, generate, Capabilities);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
    }

    /// <summary>数据库版本能力档案（同 <see cref="DatabaseAdapter"/> 上的那份，便于直接取用）。</summary>
    public OracleCapabilities Capabilities { get; }

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
