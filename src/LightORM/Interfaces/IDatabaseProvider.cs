using System.Data.Common;
using System.Text;

namespace LightORM.Interfaces
{
    public interface IDatabaseProvider
    {
        string MasterConnectionString { get; }
        ICustomDatabase CustomDatabase { get; }
        Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; }
        string[] SlaveConnectionStrings { get; }
        DbProviderFactory DbProviderFactory { get; }
    }
}
