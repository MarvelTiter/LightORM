using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Implements;
internal static class IExpressionInfoExtension
{
    public static bool IsAlreadySetSelect(this IExpressionInfo expressionInfo)
        => expressionInfo.ExpressionInfos.Values.Any(e => e.ResolveOptions == SqlResolveOptions.Select);

    public static void Update(this IExpressionInfo expressionInfo, Action<ExpressionInfo> update)
    {
        foreach (var item in expressionInfo.ExpressionInfos.Values)
        {
            update(item);
        }
    }
}



internal class ExpressionInfoProvider : IExpressionInfo
{
    public bool Completed => ExpressionInfos.Values.All(e => e.Completed);

    public Dictionary<string, ExpressionInfo> ExpressionInfos { get; }

    public ExpressionInfoProvider()
    {
        ExpressionInfos = [];
    }

    public void Add(ExpressionInfo info) => ExpressionInfos.Add(info.Id, info);
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
