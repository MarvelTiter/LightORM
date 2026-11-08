using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using LightORM.Implements;
using LightORM.Models;
using LightORM.Options;

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

    /// <summary>
    /// 从配置文件创建（由 <c>UseSqlite()</c> 空参重载登记）。
    /// <para>
    /// SQLite 没有 Server / Port / 凭据概念：用 <see cref="DatabaseDefinition.ConnectionString"/>，
    /// 或用 <see cref="DatabaseDefinition.DataSource"/>（<see cref="DatabaseDefinition.Database"/> 视作库文件路径）。
    /// </para>
    /// </summary>
    public static SqliteProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        return Create(database.ToDbOption<SqliteTableOptions>(BuildConnectionString(database.Key, definition)));
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new SQLiteConnectionStringBuilder()
                : new SQLiteConnectionStringBuilder(connectionString),
            (builder, def) =>
            {
                if (def.DataSource is { Length: > 0 } dataSource) builder.DataSource = dataSource;
                else if (def.Database is { Length: > 0 } databaseName) builder.DataSource = databaseName;
                else if (string.IsNullOrWhiteSpace(def.ConnectionString))
                {
                    throw new LightOrmException(
                        $"数据库 '{key}'：SQLite 必须提供 ConnectionString 或 DataSource（库文件路径）。" +
                        "Server / Port / UserId / Password 对 SQLite 没有意义。");
                }
            });
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
