using System.Linq;
using LightORM.Context;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Utils;

namespace LightORM.Abstracts.Builder;

internal abstract class SqlBuilder: ISqlBuilder
{
    public DbBaseType DbType { get; set; }
    public IExpressionInfo Expressions { get; } = new ExpressionInfoProvider();
    public TableEntity TableInfo { get; set; } = new();
    public string? SelectValue { get; set; }
    public Dictionary<string, object> DbParameters { get; } = [];
    public List<JoinInfo> Joins { get; set; } = [];
    public List<string> Where { get; set; } = [];
    public List<string> GroupBy { get; set; } = [];
    public List<string> OrderBy { get; set; } = [];
    public object? TargetObject { get; set; }
    protected virtual void ResolveExpressions()
    {
        if (Expressions.Completed)
        {
            return;
        }

        foreach (var item in Expressions.ExpressionInfos.Where(item => !item.Completed))
        {
            item.ResolveOptions!.DbType = DbType;
            var result = item.Expression.Resolve(item.ResolveOptions!);
            item.Completed = true;
            if (!string.IsNullOrEmpty(item.Template))
            {
                result.SqlString = string.Format(item.Template, result.SqlString);
            }
            if (item.ResolveOptions?.SqlType == SqlPartial.Where)
            {
                Where.Add(result.SqlString!);
            }
            else if (item.ResolveOptions?.SqlType == SqlPartial.Join)
            {
                var joinInfo = Joins.FirstOrDefault(j => j.ExpressionId == item.Id);
                if (joinInfo != null)
                {
                    joinInfo.Where = result.SqlString!;
                }
            }
            else if (item.ResolveOptions?.SqlType == SqlPartial.Select)
            {
                if (!string.IsNullOrWhiteSpace(result.SqlString))
                {
                    SelectValue = result.SqlString;
                }
            }
            else if (item.ResolveOptions?.SqlType == SqlPartial.GroupBy)
            {
                SelectValue = result.SqlString;
                GroupBy.Add(result.SqlString!);
            }
            else if (item.ResolveOptions?.SqlType == SqlPartial.OrderBy)
            {
                OrderBy.Add(result.SqlString!);
            }

            DbParameters.TryAddDictionary(result.DbParameters);
        }
    }
    
    protected string GetTableName(TableEntity table, bool useAlias = true)
    {
        return $"{DbType.AttachEmphasis(table.TableName!)} {( useAlias ? DbType.AttachEmphasis(table.Alias!) : "")}";
    }
    
    public abstract string ToSqlString();
}