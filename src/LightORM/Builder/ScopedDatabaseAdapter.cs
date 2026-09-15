using System.Data.Common;
using System.Text;

namespace LightORM.Builder;

internal class ScopedDatabaseAdapter(IDatabaseAdapter inner, bool quote) : IDatabaseAdapter
{
    /// <summary>被装饰的真实适配器，供能力接口(type-cast)探测钻取时使用。</summary>
    internal IDatabaseAdapter Inner => inner;

    public string Prefix => inner.Prefix;

    public string Emphasis => inner.Emphasis;

    public ISqlMethodResolver MethodResolver => inner.MethodResolver;

    public bool UseIdentifierQuote { get => inner.UseIdentifierQuote; set => inner.UseIdentifierQuote = value; }
    public bool? QuoteIdentifiers { get; set; } = quote;

    public void AddKeyWord(IEnumerable<string> keyworks)
    {
        inner.AddKeyWord(keyworks);
    }

    public string FormatBooleanValue(bool value)
    {
        return inner.FormatBooleanValue(value);
    }

    public string FormatDateTimeValue(DateTime value)
    {
        return inner.FormatDateTimeValue(value);
    }

    //public void HandleBooleanValue(StringBuilder sql, bool value)
    //{
    //    inner.HandleBooleanValue(sql, value);
    //}

    public string HandleBooleanValueForBulkCopy(bool value)
    {
        return inner.HandleBooleanValueForBulkCopy(value);
    }

    public void HandleDateValue(StringBuilder sql, DateTime value)
    {
        inner.HandleDateValue(sql, value);
    }

    public void HandleJsonColumn(JsonColumnContext context)
    {
        inner.HandleJsonColumn(context);
    }

    [Obsolete("已由 IDatabaseParameterBinder 取代, 仅为实现 IDatabaseAdapter 接口而保留转发.")]
    public void HandleJsonParameter(JsonColumnParameterContext context)
    {
        inner.HandleJsonParameter(context);
    }

    public string HandleMultipleQuerySql(string[] sqls, Dictionary<string, object> parameters)
    {
        return inner.HandleMultipleQuerySql(sqls, parameters);
    }

    public bool IsKeyWord(string keyWork)
    {
        return inner.IsKeyWord(keyWork);
    }

    public void Paging(SelectBuilder builder, StringBuilder sql)
    {
        inner.Paging(builder, sql);
    }

    public string RewriteParameterReferences(string sql, string prefix) => inner.RewriteParameterReferences(sql, prefix);

    void IDatabaseAdapter.HandleBatchInsert<T>(BatchActionContext<InsertBuilder<T>> context) => inner.HandleBatchInsert(context);

    void IDatabaseAdapter.HandleBatchUpdate<T>(BatchActionContext<UpdateBuilder<T>> context) => inner.HandleBatchUpdate(context);

    void IDatabaseAdapter.HandleInsertOrUpdate(UpsertContext context) => inner.HandleInsertOrUpdate(context);

    void IDatabaseAdapter.HandleBatchDelete<T>(BatchActionContext<DeleteBuilder<T>> context) => inner.HandleBatchDelete(context);

    void IDatabaseAdapter.HandleSelectGroupBySegment(SelectContext context) => inner.HandleSelectGroupBySegment(context);
}
