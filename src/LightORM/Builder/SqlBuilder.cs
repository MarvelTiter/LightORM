using LightORM.Extension;
using LightORM.Implements;
using System.Collections;
using System.Text;

namespace LightORM.Builder;
//internal readonly struct 
internal abstract record SqlBuilder : ISqlBuilder
{
    public static string N { get; } = Environment.NewLine;
    public ExpressionInfoProvider Expressions { get; } = new ExpressionInfoProvider();
    public TableInfo MainTable => SelectedTables[0];
    public List<TableInfo> SelectedTables { get; set; } = [];
    public int SelectedTableCount => SelectedTables.Count;
    public Dictionary<string, object> DbParameters { get; } = [];
    public List<string> Where { get; set; } = [];
    public object? TargetObject { get; set; }
    public HashSet<ResolvedValueInfo> ResolvedValues { get; set; } = [];
    public bool? IsParameterized { get; set; } = true;
    public virtual IEnumerable<TableInfo> AllTables() => [MainTable];

    public void TryAddParameters(string prefix, string sql, object? value)
    {
        if (value is null) return;
        var dic = DbParameterReader.ObjectToDictionary(prefix, sql, value);
        DbParameters.TryAddDictionary(dic);
    }
    protected virtual void BeforeResolveExpressions(ResolveContext context)
    {

    }
    protected ResolveContext? ResolveCtx { get; set; }

    protected void HandleSqlParameters(StringBuilder sql, IDatabaseAdapter database)
    {
        //var useParameterized = IsParameterized ?? ExpressionSqlOptions.Instance.Value.UseParameterized;
        //var uniqueParameters = ResolvedValues.RemoveProperty();
        foreach (var item in ResolvedValues)
        {
            if (item.Type == ExpValueType.Null || item.Value is null)
            {
                sql.ReplaceNull(item.Name);
                continue;
            }
            if (item.Type == ExpValueType.Boolean)
            {
                sql.Replace(item.Name, database.FormatBooleanValue((bool)item.Value));
            }
            else if (item.Type == ExpValueType.BooleanReverse)
            {
                sql.Replace(item.Name, database.FormatBooleanValue(!(bool)item.Value));
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
                        DbParameters[pn] = e;
                        values.Add(database.AttachPrefix(pn));
                        arrIndex++;
                    }
                    sql.Replace(item.Name, string.Join(", ", values));
                }
            }
            else
            {
                sql.Replace(item.Name, database.AttachPrefix(item.Name));
                DbParameters[item.Name] = item.Value;
            }
        }
    }
    protected void ResolveExpressions(IDatabaseAdapter database)
    {
        if (Expressions.IsCompleted)
        {
            return;
        }
        ResolveCtx = new ResolveContext(database);
        BeforeResolveExpressions(ResolveCtx);
        foreach (var item in Expressions.ExpressionInfos.Values)
        {
            if (item.IsCompleted)
                continue;
            var result = item.Expression.Resolve(item.ResolveOptions, ResolveCtx);
            if (!string.IsNullOrEmpty(item.Template))
            {
                result.SqlString = string.Format(item.Template, result.SqlString);
            }
            HandleResult(database, item, result);
            if (result.ResolvedValues != null)
                ResolvedValues.AddRange(result.ResolvedValues);
            item.IsCompleted = true;
        }
    }
    protected abstract void HandleResult(IDatabaseAdapter database, ExpressionInfo expInfo, ExpressionResolvedResult result);

    public abstract string ToSqlString(IDatabaseAdapter database);

    public static string GetTableName(IDatabaseAdapter database, TableInfo ti, bool useAlias = true, bool useEmphasis = true)
    {
        return $"{NpTableName(database, ti)}{((useAlias && !string.IsNullOrEmpty(ti.Alias)) ? $" {ti.Alias}" : "")}";
    }

    protected static string NpTableName(IDatabaseAdapter database, TableInfo table)
    {
        if (table.TableEntityInfo.IsTempTable)
        {
            return table.TableEntityInfo.TableName;
        }
        else
        {
            if (table.Schema is not null && !string.IsNullOrWhiteSpace(table.Schema))
            {
                return $"{database.AttachEmphasis(table.Schema)}.{database.AttachEmphasis(table.TableName)}";
            }
            else
            {
                return database.AttachEmphasis(table.TableName);
            }
        }
    }

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

    private static readonly Dictionary<Type, object> typeDefaults = new(8)
    {
        [typeof(int)] = 0,
        [typeof(long)] = 0,
        [typeof(double)] = 0,
        [typeof(float)] = 0,
        [typeof(DateTime)] = DateTime.UtcNow,
        [typeof(DateTimeOffset)] = DateTimeOffset.UtcNow,
        [typeof(Guid)] = Guid.NewGuid(),
        [typeof(string)] = Guid.NewGuid(),
    };
    protected static object VersionDefaultValue(Type type)
    {
        if (typeDefaults.TryGetValue(type, out var value))
        {
            return value;
        }
        throw new NotSupportedException("不支持的Version列类型");
    }
}