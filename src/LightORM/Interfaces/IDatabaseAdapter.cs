using System.Data.Common;
using System.Text;

namespace LightORM.Interfaces
{
    public interface IDatabaseAdapter
    {
        string Prefix { get; }
        string Emphasis { get; }
        ISqlMethodResolver MethodResolver { get; }
        /// <summary>
        /// 是否使用标识引用符号，数据库关键词会强制使用，可通过<see cref="AddKeyWord"/>新增
        /// </summary>
        bool UseIdentifierQuote { get; set; }

        /// <summary>
        /// 单次语句构建是否使用标识引用符号
        /// </summary>
        bool? QuoteIdentifiers { get; set; }
        ///// <summary>
        ///// 获取删除语句的模板，将提供两个参数，{0} 表示表名，{1} 表示别名
        ///// </summary>
        //string DeleteTemplate { get; }
        internal void Paging(SelectBuilder builder, StringBuilder sql);
        //void HandleBooleanValue(StringBuilder sql, bool value);
        string FormatBooleanValue(bool value);
        string FormatDateTimeValue(DateTime value);
        void HandleDateValue(StringBuilder sql, DateTime value);
        string HandleBooleanValueForBulkCopy(bool value);

        bool IsKeyWord(string keyWork);
        void AddKeyWord(IEnumerable<string> keyworks);

        string HandleMultipleQuerySql(string[] sqls, Dictionary<string, object> parameters);
        string RewriteParameterReferences(string sql, string prefix);
        void HandleJsonColumn(JsonColumnContext context);

        /// <summary>
        /// [历史兼容] 曾用于为 JSON 列参数在 SQL 占位符上补 ::json/::jsonb 及做值序列化。
        /// 已被 <see cref="IDatabaseParameterBinder"/> 取代(参数携带列元数据后由方言在绑定阶段类型化)。
        /// 保留仅为不破坏第三方自定义 adapter; 新实现请改用 IDatabaseParameterBinder。
        /// </summary>
        [Obsolete("已由 IDatabaseParameterBinder 取代, 框架内不再调用; 请改为实现 IDatabaseParameterBinder.")]
        void HandleJsonParameter(JsonColumnParameterContext context);

        internal void HandleSelectGroupBySegment(SelectContext context);

        internal void HandleInsertOrUpdate(UpsertContext context);

        internal void HandleBatchInsert<T>(BatchActionContext<InsertBuilder<T>> context);

        internal void HandleBatchUpdate<T>(BatchActionContext<UpdateBuilder<T>> context);

        internal void HandleBatchDelete<T>(BatchActionContext<DeleteBuilder<T>> context);
    }
}
