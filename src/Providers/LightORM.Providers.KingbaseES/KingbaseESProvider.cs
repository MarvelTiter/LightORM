using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using System.Data;
using System.Data.Common;

namespace LightORM.Providers.KingbaseES;

public class KingbaseESProvider : BaseDatabaseProvider
{
    public static readonly DbBaseType KingbaseEs = new("KingbaseES");
    public static KingbaseESProvider Create(DataBaseOption option) => new(option);

    public static KingbaseESProvider Create(Action<DataBaseOption> setting)
    {
        var dbOption = new DataBaseOption();
        setting.Invoke(dbOption);
        if (string.IsNullOrEmpty(dbOption.MasterConnectionString))
        {
            throw new ArgumentNullException(nameof(dbOption.MasterConnectionString), "连接字符串不能为空");
        }
        return Create(dbOption);
    }
    private KingbaseESProvider(DataBaseOption option) : base(option.MasterConnectionString!, option.SalveConnectionStrings)
    {
        DbHandler = new KingbaseESTableHandler(option.GenerateOption);
        var sqlMethodResolver = new KingbaseESMethodResolver();
        option.SqlMethodConfiguration?.Invoke(sqlMethodResolver);
        DatabaseAdapter = new CustomKingbaseESAdapter(sqlMethodResolver, option.GenerateOption);
        DatabaseAdapter.AddKeyWord(option.Keyworks);
        DatabaseAdapter.UseIdentifierQuote = option.IsUseIdentifierQuote;
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
