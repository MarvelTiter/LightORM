using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Implements
{
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
}
