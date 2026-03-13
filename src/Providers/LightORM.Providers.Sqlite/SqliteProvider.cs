using LightORM.Interfaces;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using LightORM.Implements;
using System.ComponentModel.DataAnnotations;
using LightORM.Models;

namespace LightORM.Providers.Sqlite;

public sealed class SqliteProvider : BaseDatabaseProvider
{
    public static SqliteProvider Create(DataBaseOption option) => new(option);

    public static SqliteProvider Create(Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption(new SqliteMethodResolver());
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }

    private SqliteProvider(DataBaseOption option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new SqliteTableHandler(option.GenerateOption);
        CustomDatabase = new CustomSqlite(option.MethodResolver, option.GenerateOption);
        CustomDatabase.AddKeyWord(option.Keyworks);
        CustomDatabase.UseIdentifierQuote = option.IsUseIdentifierQuote;
        DbProviderFactory = option.NewFactory ?? SQLiteFactory.Instance;
    }

    public override DbBaseType DbBaseType => DbBaseType.Sqlite;

    public override DbProviderFactory DbProviderFactory { get; }

    public override ICustomDatabase CustomDatabase { get; }

    public override Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler { get; }

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotSupportedException();
    }


}
