using System.Data.Common;
using System.Text;

namespace LightORM.Interfaces
{
    public interface IDatabaseProvider
    {
        string MasterConnectionString { get; }
        string[] SlaveConnectionStrings { get; }
        DbProviderFactory DbProviderFactory { get; }
    }
}
