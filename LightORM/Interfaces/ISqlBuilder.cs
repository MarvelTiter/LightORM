using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Interfaces
{
    internal interface ISqlBuilder
    {
        /// <summary>
        /// 表达式
        /// </summary>
        IExpressionInfo Expressions { get; }

        /// <summary>
        /// 实体信息
        /// </summary>
        TableEntity TableInfo { get; }

        /// <summary>
        /// 参数信息
        /// </summary>
        Dictionary<string, object> DbParameters { get; }

        /// <summary>
        /// 到Sql字符串
        /// </summary>
        /// <returns></returns>
        string ToSqlString();
    }
}
