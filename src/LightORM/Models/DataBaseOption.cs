using System.Data.Common;

namespace LightORM.Models;

public sealed class DataBaseOption<T> : IDbOption
    where T : TableOptions, new()
{
    public string? DbKey { get; set; }
    public string? MasterConnectionString { get; set; }
    public string[]? SalveConnectionStrings { get; set; }
    public DbProviderFactory? NewFactory { get; set; }
    public T GenerateOption { get; set; } = new();
    public HashSet<string> Keyworks { get; set; } = [];
    public bool? IsUseIdentifierQuote { get; private set; }
    public Action<ISqlMethodResolver>? SqlMethodConfiguration { get; set; }

    public IDbOption ConfigurationMethodResolver(Action<ISqlMethodResolver> action)
    {
        SqlMethodConfiguration = action;
        return this;
    }
    public IDbOption AddDbKeyWords(params string[] keyWords)
    {
        Keyworks.UnionWith(keyWords);
        return this;
    }

    public IDbOption UseIdentifierQuote(bool value = true)
    {
        IsUseIdentifierQuote = value;
        return this;
    }

    public IDbOption OverrideDbProviderFactory(DbProviderFactory factory)
    {
        NewFactory = factory;
        return this;
    }

    TOption IDbOption.GetOption<TOption>()
    {
        return (GenerateOption as TOption)!;
    }

    //public IDbOption TableConfiguration(Action<TableOptions> action)
    //{
    //    action.Invoke(GenerateOption);
    //    return this;
    //}
}
