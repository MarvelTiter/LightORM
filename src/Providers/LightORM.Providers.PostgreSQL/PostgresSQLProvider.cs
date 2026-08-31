using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using LightORM.Implements;
using LightORM.Models;

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