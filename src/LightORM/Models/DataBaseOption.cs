using System.Data.Common;

namespace LightORM.Models;

public sealed class DataBaseOption : IDbOption
{
    private readonly ICustomDatabase database;

    [Obsolete]
    public DataBaseOption(ISqlMethodResolver methodResolver)
    {
        MethodResolver = methodResolver;
    }

    public DataBaseOption(ICustomDatabase database)
    {
        this.database = database;
        MethodResolver = database.MethodResolver;
    }
    
    public string? DbKey { get; set; }
    public string? MasterConnectionString { get; set; }
    public string[]? SalveConnectionStrings { get; set; }
    public ISqlMethodResolver MethodResolver { get; }
    public DbProviderFactory? NewFactory { get; set; }

    public void AddDbKeyWords(params string[] keyWords)
    {
        database.AddKeyWord(keyWords);
    }

    public IDbOption UseIdentifierQuote(bool value = true)
    {
        database.UseIdentifierQuote = value;
        return this;
    }

    public void OverrideDbProviderFactory(DbProviderFactory factory)
    {
        NewFactory = factory;
    }
}
