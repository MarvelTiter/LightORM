
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Interfaces
{
    internal interface IExpressionInfo
    {
        /// <summary>
        /// 解析完成
        /// </summary>
        bool Completed {  get; }

        /// <summary>
        /// 表达式信息
        /// </summary>
        List<ExpressionInfo> ExpressionInfos { get; }

        void Add(ExpressionInfo info);
        void Update(string? id, Action<ExpressionInfo> update);
        void Update(string? id, ExpressionInfo? info);
    }
}
