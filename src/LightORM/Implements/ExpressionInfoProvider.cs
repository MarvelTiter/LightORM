using System.Collections.Concurrent;
using System.Text;

namespace LightORM.Implements;
internal static class IExpressionInfoExtension
{
    public static bool IsAlreadySetSelect(this ExpressionInfoProvider expressionInfo)
        => expressionInfo.ExpressionInfos.Values.Any(e => e.ResolveOptions == SqlResolveOptions.Select);

    public static void Update(this ExpressionInfoProvider expressionInfo, Action<ExpressionInfo> update)
    {
        foreach (var item in expressionInfo.ExpressionInfos.Values)
        {
            update(item);
        }
    }
}

internal class ExpressionInfoProvider //: IExpressionInfo
{
    private static readonly ConcurrentDictionary<string, string> cacheResults = [];
    public bool Completed => ExpressionInfos.Values.All(e => e.Completed);

    private readonly StringBuilder labels = new();

    public Dictionary<string, ExpressionInfo> ExpressionInfos { get; }

    public string Key => labels.ToString();

    public ExpressionInfoProvider()
    {
        ExpressionInfos = [];
    }

    public bool TryHitCache(IEnumerable<TableInfo> tables, out string? sql)
    {
        var key = $"{string.Join("-", tables.Select(t => t.TableEntityInfo.TableName))}_{Key}";
        return cacheResults.TryGetValue(key, out sql);
    }

    public void CacheResult(IEnumerable<TableInfo> tables, string sql)
    {
        var key = $"{string.Join("-", tables.Select(t => t.TableEntityInfo.TableName))}_{Key}";
        cacheResults.TryAdd(key, sql);
    }

    public void Add(ExpressionInfo info)
    {
        labels.Append(info.Expression?.ToString());
        ExpressionInfos.Add(info.Id, info);
    }
    public void Remove(ExpressionInfo info) => ExpressionInfos.Remove(info.Id);

    public void Clear() => ExpressionInfos.Clear();
    //public void Update(string? id, Action<ExpressionInfo> update)
    //{
    //    var exp = ExpressionInfos.FirstOrDefault(info => info.Id == id);
    //    if (exp != null)
    //    {
    //        var newExp = exp with { };
    //        update.Invoke(newExp);
    //        //Update(id, newExp);
    //    }
    //}

    //public void Update(string? id, ExpressionInfo? info)
    //{
    //    var exp = ExpressionInfos.FirstOrDefault(info => info.Id == id);
    //    if (exp != null && info != null)
    //    {
    //        ExpressionInfos.Remove(exp);
    //        Add(info);
    //    }
    //}
}
