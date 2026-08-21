using System.Data.Common;
using System.Text;

namespace LightORM.Builder;

internal class ScopedDatabaseAdapter(IDatabaseAdapter inner, bool quote) : IDatabaseAdapter
{
    public string Prefix => inner.Prefix;

    public string Emphasis => inner.Emphasis;

    public ISqlMethodResolver MethodResolver => inner.MethodResolver;

    public bool UseIdentifierQuote { get => inner.UseIdentifierQuote; set => inner.UseIdentifierQuote = value; }
    public bool? QuoteIdentifiers { get; set; } = quote;

    public void AddKeyWord(IEnumerable<string> keyworks)
    {
        inner.AddKeyWord(keyworks);
    }

    public void DbCommandInit(DbCommand dbCommand)
    {
        inner.DbCommandInit(dbCommand);
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

    public void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        inner.Paging(builder, sql);
    }

    public void ReturnIdentitySql(StringBuilder sql)
    {
        inner.ReturnIdentitySql(sql);
    }

    public string RewriteParameterReferences(string sql, string prefix) => inner.RewriteParameterReferences(sql, prefix);

    void IDatabaseAdapter.HandleBatchInsert(BatchActionContext context) => inner.HandleBatchInsert(context);

    void IDatabaseAdapter.HandleBatchUpdate(BatchActionContext context) => inner.HandleBatchUpdate(context);

    void IDatabaseAdapter.HandleInsertOrUpdate(UpsertContext context) => inner.HandleInsertOrUpdate(context);

    void IDatabaseAdapter.HandleBatchDelete<T>(BatchActionContext<DeleteBuilder<T>> context) => inner.HandleBatchDelete(context);
}
