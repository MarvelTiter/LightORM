using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteProvider : BaseDatabaseProvider
{
    public static SqliteProvider Create(DataBaseOption<SqliteTableOptions> option) => new(option);

    public static SqliteProvider Create(Action<DataBaseOption<SqliteTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<SqliteTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    private SqliteProvider(DataBaseOption<SqliteTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var generate = option.GenerateOption;
        var factory = option.NewFactory ?? SQLiteFactory.Instance;
        // 能力档案先建：handler / methodResolver / adapter 都要读它。
        // 探测在构造期发起（不阻塞）；SQLite 不 Open，故探测不会建出库文件。
        DbProviderFactory = factory;
        Capabilities = SqliteCapabilities.Start(factory, MasterConnectionString, generate.SpecificVersion, generate.DetectVersion);
        DbHandler = new SqliteTableHandler(generate, Capabilities);
        var sqlMethodResolver = new SqliteMethodResolver(generate, Capabilities);
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomSqliteAdapter(sqlMethodResolver, generate, Capabilities);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
    }

    public override DbBaseType DbBaseType => DbBaseType.Sqlite;

    /// <summary>
    /// 版本能力档案：由探测结果 / <see cref="TableOptions.SpecificVersion"/> 推导；
    /// 未启用探测、探测失败或版本未知时按<b>完整功能</b>。
    /// </summary>
    public SqliteCapabilities Capabilities { get; }

    public override DbProviderFactory DbProviderFactory { get; }

    public override IDatabaseAdapter DatabaseAdapter { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; }

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotSupportedException();
    }


}
