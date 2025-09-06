using System.Data.Common;

namespace LightORM.Implements;

public abstract class BaseDatabaseProvider : IDatabaseProvider
{
    public BaseDatabaseProvider(string masterConnectionString, string[] slaveConnectionStrings)
    {
        MasterConnectionString = masterConnectionString;
        SlaveConnectionStrings = slaveConnectionStrings;
    }
    public abstract DbBaseType DbBaseType { get; }
    public string MasterConnectionString { get; }
    public abstract ICustomDatabase CustomDatabase { get; }
    
    public abstract IDatabaseTableHandler DbHandler { get; }
    
    public abstract Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; }
    public string[] SlaveConnectionStrings { get; }
    public abstract DbProviderFactory DbProviderFactory { get; set; }
    public abstract int BulkCopy(DataTable dataTable);
}