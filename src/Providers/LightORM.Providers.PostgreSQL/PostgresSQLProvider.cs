using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;
using LightORM.Options;

namespace LightORM.Providers.PostgreSQL;

public sealed class PostgreSQLProvider : BaseDatabaseProvider
{
    public static PostgreSQLProvider Create(DataBaseOption<PostgreSQLTableOptions> option) => new(option);

    public static PostgreSQLProvider Create(Action<DataBaseOption<PostgreSQLTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<PostgreSQLTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    /// <summary>
    /// 从配置文件创建（由 <c>UsePostgreSQL()</c> 空参重载登记）。
    /// <para>
    /// <see cref="DatabaseDefinition.ConnectionString"/> 为底，分散字段逐项覆盖；
    /// 没有连接串时用 Host / Port / Username / Password / Database 拼装。
    /// </para>
    /// </summary>
    public static PostgreSQLProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        var dbOption = database.ToDbOption<PostgreSQLTableOptions>(BuildConnectionString(database.Key, definition));

        // 方言私有项走 Settings，不进通用模型
        if (definition.GetSetting("TableSpace") is { Length: > 0 } tableSpace)
        {
            dbOption.GenerateOption.TableSpace = tableSpace;
        }

        return Create(dbOption);
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new Npgsql.NpgsqlConnectionStringBuilder()
                : new Npgsql.NpgsqlConnectionStringBuilder(connectionString),
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

    public override DbBaseType DbBaseType => DbBaseType.PostgreSQL;

    private PostgreSQLProvider(DataBaseOption<PostgreSQLTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        var sqlMethodResolver = new PostgreSQLMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomPostgreSQL(sqlMethodResolver, option.GenerateOption);
        DbHandler = new PostgreSQLTableHandler(option.GenerateOption, DatabaseAdapter);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? false;
        DbProviderFactory = option.NewFactory ?? Npgsql.NpgsqlFactory.Instance;
    }

    public override IDatabaseAdapter DatabaseAdapter { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; }
    public override DbProviderFactory DbProviderFactory { get; }

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotSupportedException();
    }
}