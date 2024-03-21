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
    protected IDbHelper DbHelper => DbType.GetDbHelper();
    protected string AttachPrefix(string content) => DbType.AttachPrefix(content);
    protected string AttachEmphasis(string content) => DbType.AttachEmphasis(content);
    protected void ResolveExpressions()
    {
        if (Expressions.Completed)
        {
            return;
        }
        var dbParameterStartIndex = 0;
        foreach (var item in Expressions.ExpressionInfos.Where(item => !item.Completed))
        {
            item.ResolveOptions!.DbType = DbType;
            item.DbParameterIndex += dbParameterStartIndex;
            var result = item.Expression.Resolve(item.ResolveOptions!);
            dbParameterStartIndex = item.DbParameterIndex;
            item.Completed = true;
            if (!string.IsNullOrEmpty(item.Template))
            {
                result.SqlString = string.Format(item.Template, result.SqlString);
            }

            HandleResult(item, result);

            DbParameters.TryAddDictionary(result.DbParameters);
        }
    }

    protected string GetTableName(TableEntity table, bool useAlias = true)
    {
        return $"{AttachEmphasis(table.TableName!)} {(useAlias ? AttachEmphasis(table.Alias!) : "")}";
    }
    protected abstract void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result);

    public abstract string ToSqlString();
}