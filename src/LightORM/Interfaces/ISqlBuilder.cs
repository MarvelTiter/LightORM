using LightORM.Implements;

namespace LightORM.Interfaces
{
    public interface ISqlBuilder
    {
        ///// <summary>
        ///// 数据库类型
        ///// </summary>
        //DbBaseType DbType { get; set; }
        /// <summary>
        /// 表达式
        /// </summary>
        internal ExpressionInfoProvider Expressions { get; }
        /// <summary>
        /// SELECT 或者 JOIN 的表
        /// </summary>
        List<TableInfo> SelectedTables { get; set; }
        /// <summary>
        /// 实体信息
        /// </summary>
        TableInfo MainTable { get; }
        //List<ITableEntityInfo> OtherTables { get; }
        //ITableEntityInfo[] AllTables { get; }
        /// <summary>
        /// 参数信息
        /// </summary>
        Dictionary<string, object> DbParameters { get; }
        /// <summary>
        /// 是否参数化
        /// </summary>
        internal bool? IsParameterized { get; set; }
        /// <summary>
        /// 到Sql字符串
        /// </summary>
        /// <returns></returns>
        string ToSqlString(ICustomDatabase database);
    }

    public interface ISelectSqlBuilder : ISqlBuilder
    {
        //int PageIndex { get; set; }
        //int PageSize { get; set; }
        int Skip {  get; set; }
        int Take { get; set; }
        object? AdditionalValue { get; set; }
        List<string> GroupBy { get; set; }
        List<string> OrderBy { get; set; }
    }
}
