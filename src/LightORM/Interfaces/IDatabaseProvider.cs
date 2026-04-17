using System.Data.Common;

namespace LightORM.Interfaces
{
    public interface IDatabaseProvider
    {
        DbBaseType DbBaseType { get; }
        string MasterConnectionString { get; }
        IDatabaseAdapter DatabaseAdapter { get; }
        IDatabaseTableHandler DbHandler { get; }
        [Obsolete]
        Func<TableOptions, IDatabaseTableHandler>? TableHandler { get; }
        string[]? SlaveConnectionStrings { get; }
        DbProviderFactory DbProviderFactory { get; }
        int BulkCopy(DataTable dataTable);
    }
}
