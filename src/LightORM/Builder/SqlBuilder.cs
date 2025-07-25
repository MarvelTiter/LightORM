using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace LightORM.Builder;

internal abstract record SqlBuilder : ISqlBuilder
{
    protected SqlBuilder(DbBaseType type)
    {
        DbType = type;
        dbHelperLazy = new Lazy<ICustomDatabase>(() => DbType.GetDbCustom());
    }
    public static string N { get; } = Environment.NewLine;
    public DbBaseType DbType { get; set; }

    public ExpressionInfoProvider Expressions { get; } = new ExpressionInfoProvider();
    public TableInfo MainTable => SelectedTables[0];
    public List<TableInfo> SelectedTables { get; set; } = [];
    public int SelectedTableCount => SelectedTables.Count;
    public Dictionary<string, object> DbParameters { get; } = [];
    public List<string> Where { get; set; } = [];
    public object? TargetObject { get; set; }
    public List<string> Members { get; set; } = [];
    public List<DbParameterInfo> DbParameterInfos { get; set; } = [];
    private readonly Lazy<ICustomDatabase> dbHelperLazy;
    public ICustomDatabase DbHelper => dbHelperLazy.Value;
    public string AttachPrefix(string content) => DbHelper.AttachPrefix(content);
    public string AttachEmphasis(string content) => DbHelper.AttachEmphasis(content);
    //public int DbParameterStartIndex { get; set; }
    public virtual IEnumerable<TableInfo> AllTables() => [MainTable];

    public void TryAddParameters(string sql, object? value)
    {
        if (value is null) return;
        var dic = DbParameterReader.ReadToDictionary(sql, value);
        DbParameters.TryAddDictionary(dic);
    }
    protected virtual void BeforeResolveExpressions(ResolveContext context)
    {

    }
    protected ResolveContext? ResolveCtx { get; set; }
    protected void HandleSqlParameters(StringBuilder sql)
    {
        //var useParameterized = ExpressionSqlOptions.Instance.Value.UseParameterized;
        foreach (var item in DbParameterInfos)
        {
            if (item.Type == ExpValueType.Null || item.Value is null)
            {
                var parameterIndex = sql.IndexOf(item.Name);

                if (sql[parameterIndex - 2] == '=')
                {
                    //equal
                    sql.Replace($"= {item.Name}", "IS NULL");
                }
                else if (sql[parameterIndex - 3] == '<' && sql[parameterIndex - 2] == '>')
                {
                    //not equal
                    sql.Replace($"<> {item.Name}", "IS NOT NULL");
                }
                continue;
            }
            if (item.Type == ExpValueType.Boolean)
            {
                sql.Replace(item.Name, DbHelper.HandleBooleanValue((bool)item.Value));
            }
            else if (item.Type == ExpValueType.BooleanReverse)
            {
                sql.Replace(item.Name, DbHelper.HandleBooleanValue(!(bool)item.Value));
            }
            else if (item.Type == ExpValueType.Collection)
            {
                if (item.Value is IEnumerable ema)
                {
                    List<string> values = [];
                    int arrIndex = 0;
                    foreach (var e in ema)
                    {
                        var pn = $"{item.Name}_{arrIndex}";
                        DbParameters.Add(pn, e);
                        values.Add(DbHelper.AttachPrefix(pn));
                        arrIndex++;
                    }
                    sql.Replace(item.Name, string.Join(", ", values));
                }
            }
            else
            {
                sql.Replace(item.Name, DbHelper.AttachPrefix(item.Name));
                DbParameters.Add(item.Name, item.Value);
            }
        }
    }
    protected void ResolveExpressions()
    {
        if (Expressions.Completed)
        {
            return;
        }
        ResolveCtx = new ResolveContext(DbHelper);
        BeforeResolveExpressions(ResolveCtx);
        var index = 0;
        foreach (var item in Expressions.ExpressionInfos.Values.Where(item => !item.Completed))
        {
            //item.ResolveOptions!.DbType = DbType;
            item.ResolveOptions!.ParameterPartialIndex = index;
            var result = item.Expression.Resolve(item.ResolveOptions!, ResolveCtx);
            item.Completed = true;
            if (!string.IsNullOrEmpty(item.Template))
            {
                result.SqlString = string.Format(item.Template, result.SqlString);
            }
            HandleResult(item, result);
            DbParameterInfos.AddRange(result.DbParameters ?? []);
            index++;
        }
    }

    public string GetTableName(TableInfo ti, bool useAlias = true, bool useEmphasis = true)
    {
        return $"{NpTableName(ti.TableEntityInfo)}{((useAlias && !string.IsNullOrEmpty(ti.Alias)) ? $" {AttachEmphasis(ti.Alias)}" : "")}";
    }

    //TODO Oracle?
    protected string NpTableName(ITableEntityInfo table)
    {
        if (table.IsTempTable)
        {
            return table.TableName;
        }
        var tablename = table.TableName;
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

    protected static object VersionPlus(object? oldVersion)
    {
        return oldVersion switch
        {
            int i => i + 1,
            long l => l + 1,
            double d => d + 1,
            float f => f + 1,
            DateTime => DateTime.UtcNow,
            DateTimeOffset => DateTimeOffset.UtcNow,
            Guid => Guid.NewGuid(),
            string s when Guid.TryParse(s, out _) => Guid.NewGuid().ToString(),
            _ => throw new NotSupportedException("不支持的Version列类型"),
        };
    }
}