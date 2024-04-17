using System.Linq;
using LightORM.ExpressionSql.DbHandle;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Utils;

namespace LightORM.Builder;

internal abstract class SqlBuilder : ISqlBuilder
{
    public DbBaseType DbType { get; set; }
    public IExpressionInfo Expressions { get; } = new ExpressionInfoProvider();
    public TableEntity TableInfo { get; set; } = new();
    public Dictionary<string, object> DbParameters { get; } = [];
    public List<string> Where { get; set; } = [];
    public object? TargetObject { get; set; }
    public List<string> Members { get; set; } = [];
    public IDbHelper DbHelper => DbType.GetDbHelper();
    public string AttachPrefix(string content) => DbType.AttachPrefix(content);
    public string AttachEmphasis(string content) => DbType.AttachEmphasis(content);
    public int DbParameterStartIndex { get; set; }
    protected void ResolveExpressions()
    {
        if (Expressions.Completed)
        {
            return;
        }
        foreach (var item in Expressions.ExpressionInfos.Where(item => !item.Completed))
        {
            item.ResolveOptions!.DbType = DbType;
            item.ResolveOptions!.ParameterIndex = DbParameterStartIndex;
            var result = item.Expression.Resolve(item.ResolveOptions!);
            DbParameterStartIndex = item.ResolveOptions!.ParameterIndex;
            item.Completed = true;
            if (!string.IsNullOrEmpty(item.Template))
            {
                result.SqlString = string.Format(item.Template, result.SqlString);
            }

            HandleResult(item, result);

            DbParameters.TryAddDictionary(result.DbParameters);
        }
    }

    public string GetTableName(TableEntity table, bool useAlias = true)
    {
        return $"{NpTableName(table.TableName!)}{((useAlias && !string.IsNullOrEmpty(table.Alias)) ? $" {AttachEmphasis(table.Alias!)}" : "")}";
    }

    //TODO Oracle?
    private string NpTableName(string tablename)
    {
        if (!tablename.Contains('.'))
        {
            return AttachEmphasis(tablename);
        }
        else
        {
            var prs = tablename.Split('.');
            return string.Join(".", prs.Select(AttachEmphasis));
        }
    }

    protected abstract void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result);

    public abstract string ToSqlString();
}