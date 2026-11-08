using Dm;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Options;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.Dameng;

public sealed class DamengProvider : BaseDatabaseProvider
{
    public static readonly DbBaseType Dameng = new("Dameng");
    public static DamengProvider Create(DataBaseOption<DamengTableOptions> option) => new(option);
    public static DamengProvider Create(Action<DataBaseOption<DamengTableOptions>> setting)
    {
        var dbOption = new DataBaseOption<DamengTableOptions>();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    /// <summary>
    /// 从配置文件创建（由 <c>UseDameng()</c> 空参重载登记）。
    /// <para>
    /// 语义：<see cref="DatabaseDefinition.ConnectionString"/> 为底，分散字段逐项覆盖；
    /// 没有连接串时用 Server / Port / UserId / Password 拼装。
    /// </para>
    /// </summary>
    public static DamengProvider Create(DatabaseOptions database)
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        var dbOption = database.ToDbOption<DamengTableOptions>(BuildConnectionString(database.Key, definition));

        var generate = dbOption.GenerateOption;

        // 方言私有配置：键名归 Dameng provider 自己定义，不进通用模型
        if (definition.GetSetting("TableSpace") is { Length: > 0 } tableSpace)
        {
            generate.TableSpace = tableSpace;
        }

        if (definition.GetSetting("GenerateUserId") is { Length: > 0 } generateUserId)
        {
            generate.UserId = generateUserId;
        }

        return Create(dbOption);
    }

    private static string BuildConnectionString(string? key, DatabaseDefinition definition)
    {
        return DatabaseConnectionString.Build(key, definition,
            connectionString => connectionString is null
                ? new DmConnectionStringBuilder()
                : new DmConnectionStringBuilder(connectionString),
            (builder, def) =>
            {
                if (def.DataSource is { Length: > 0 } dataSource)
                {
                    builder.Server = dataSource;
                    if (def.Port is { } dataSourcePort) builder.Port = dataSourcePort;
                }
                else if (def.Server is { Length: > 0 } server)
                {
                    builder.Server = server;
                    builder.Port = def.Port ?? 5236;
                }
                else if (def.Port is { } port)
                {
                    builder.Port = port;
                }

                if (def.UserId is { Length: > 0 } userId) builder.User = userId;
                if (def.Password is { } password) builder.Password = password;
                if (def.Database is { Length: > 0 } databaseName) builder.Database = databaseName;
            });
    }

    private DamengProvider(DataBaseOption<DamengTableOptions> option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new DamengTableHandler(option.GenerateOption);
        var sqlMethodResolver = new DamengMethodResolver(option.GenerateOption);
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomDamengAdapter(sqlMethodResolver, option.GenerateOption);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote ?? true;
        DbProviderFactory = option.NewFactory ?? DmClientFactory.Instance;
    }

    public override DbBaseType DbBaseType => Dameng;

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
        using var conn = (DmConnection)DbProviderFactory.CreateConnection()!;
        conn.ConnectionString = MasterConnectionString;
        conn.Open();

        using var transcation = (DmTransaction)conn.BeginTransaction();
        var bulkCopy = new DmBulkCopy(conn, DmBulkCopyOptions.Default, transcation)
        {
            DestinationTableName = DatabaseAdapter.Emphasis.Insert(1, dataTable.TableName),
            BulkCopyTimeout = 120
        };

        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            var col = dataTable.Columns[i];
            var mapping = new DmBulkCopyColumnMapping(i, DatabaseAdapter.Emphasis.Insert(1, col.ColumnName));
            bulkCopy.ColumnMappings.Add(mapping);
        }
        int effectedRows = 0;
        try
        {
            bulkCopy.WriteToServer(dataTable);
            transcation.Commit();
        }
        catch
        {
            transcation.Rollback();
            effectedRows = 0;
            throw;
        }
        finally
        {
            transcation.Dispose();
            conn.Close();
        }
        return effectedRows;
    }

}
