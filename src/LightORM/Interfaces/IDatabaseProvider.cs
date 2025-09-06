using System.Data.Common;

namespace LightORM.Interfaces
{
    public interface IDatabaseProvider
    {
        DbBaseType DbBaseType { get; }
        string MasterConnectionString { get; }
        ICustomDatabase CustomDatabase { get; }
        IDatabaseTableHandler DbHandler { get; }
        [Obsolete]
        Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; }
        string[] SlaveConnectionStrings { get; }
        DbProviderFactory DbProviderFactory { get; }
        int BulkCopy(DataTable dataTable);
    }
}
