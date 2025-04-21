using System.Data.Common;

namespace LightORM.Models;

public sealed class DataBaseOption(ISqlMethodResolver methodResolver): IDbOption
{
    public string? DbKey { get; set; }
    public string? MasterConnectionString { get; set; }
    public string[]? SalveConnectionStrings { get; set; }
    public ISqlMethodResolver MethodResolver { get; } = methodResolver;
    public DbProviderFactory? NewFactory { get; set; }
    public void OverrideDbProviderFactory(DbProviderFactory factory)
    {
        NewFactory = factory;
    }
}
