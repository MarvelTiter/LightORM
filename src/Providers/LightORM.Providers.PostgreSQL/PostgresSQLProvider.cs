using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Implements;
using LightORM.Models;

namespace LightORM.Providers.PostgreSQL;

public sealed class PostgreSQLProvider : BaseDatabaseProvider
{
    public static PostgreSQLProvider Create(DataBaseOption option) => new(option);

    public static PostgreSQLProvider Create(Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    public override DbBaseType DbBaseType => DbBaseType.PostgreSQL;

    private PostgreSQLProvider(DataBaseOption option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new PostgreSQLTableHandler(option.GenerateOption);
        var sqlMethodResolver = new PostgreSQLMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        CustomDatabase = new CustomPostgreSQL(sqlMethodResolver, option.GenerateOption);
        CustomDatabase.AddKeyWord(option.Keyworks);
        CustomDatabase.UseIdentifierQuote = option.IsUseIdentifierQuote;
        DbProviderFactory = option.NewFactory ?? Npgsql.NpgsqlFactory.Instance;
    }

    public override ICustomDatabase CustomDatabase { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; }
    public override DbProviderFactory DbProviderFactory { get; }

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotSupportedException();
    }
}