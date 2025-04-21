using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Interfaces
{
    public interface ISqlBuilder
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        DbBaseType DbType { get; set; }
        /// <summary>
        /// 表达式
        /// </summary>
        internal IExpressionInfo Expressions { get; }
        List<ITableEntityInfo> SelectedTables { get; set; }
        /// <summary>
        /// 实体信息
        /// </summary>
        ITableEntityInfo MainTable { get; }
        //List<ITableEntityInfo> OtherTables { get; }
        ITableEntityInfo[] AllTables { get; }
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

    public interface ISelectSqlBuilder : ISqlBuilder
    {
        int PageIndex { get; set; }
        int PageSize { get; set; }
        object? AdditionalValue { get; set; }
        List<string> GroupBy { get; set; }
        List<string> OrderBy { get; set; }
    }
}
