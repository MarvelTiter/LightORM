using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Options;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.KingbaseES;

public class KingbaseESProvider : BaseDatabaseProvider
{
    public static readonly DbBaseType KingbaseEs = new("KingbaseES");
    public static KingbaseESProvider Create(DataBaseOption<KingbaseESTableOptions> option) => new(option);

    public static KingbaseESProvider Create(Action<DataBaseOption<KingbaseESTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<KingbaseESTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }
    /// <summary>
    /// 从配置文件创建（由 <c>UseKingbaseES()</c> 空参重载登记）。
    /// <para>
    /// <see cref="DatabaseDefinition.ConnectionString"/> 为底，分散字段逐项覆盖；
    /// 没有连接串时用 Host（Server/DataSource）/ Port / Username / Password / Database 拼装。
    /// </para>
    /// </summary>
    public static KingbaseESProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        return Create(database.ToDbOption<KingbaseESTableOptions>(BuildConnectionString(database.Key, definition)));
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new Kdbndp.KdbndpConnectionStringBuilder()
                : new Kdbndp.KdbndpConnectionStringBuilder(connectionString),
            (builder, def) =>
            {
                if (def.DataSource is { Length: > 0 } dataSource) builder.Host = dataSource;
                else if (def.Server is { Length: > 0 } host) builder.Host = host;

                if (def.Port is { } port) builder.Port = port;
                if (def.UserId is { Length: > 0 } userName) builder.Username = userName;
                if (def.Password is { } password) builder.Password = password;
                if (def.Database is { Length: > 0 } databaseName) builder.Database = databaseName;
            });
    }

    private KingbaseESProvider(DataBaseOption<KingbaseESTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new KingbaseESTableHandler(option.GenerateOption);
        var sqlMethodResolver = new KingbaseESMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomKingbaseESAdapter(sqlMethodResolver, option.GenerateOption);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
        DbProviderFactory = option.NewFactory ?? Kdbndp.KdbndpFactory.Instance;
    }
    public override DbBaseType DbBaseType => KingbaseEs;

    public override IDatabaseAdapter DatabaseAdapter { get; }

    public override IDatabaseTableHandler DbHandler { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; }

    public override DbProviderFactory DbProviderFactory { get; }

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotImplementedException();
    }
}
