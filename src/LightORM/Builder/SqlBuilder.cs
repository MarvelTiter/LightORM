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
    public List<DbParameterInfo> DbParameterInfos { get; set; } = [];
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

    protected void HandleSqlParameters(StringBuilder sql, ICustomDatabase database)
    {
        //var useParameterized = IsParameterized ?? ExpressionSqlOptions.Instance.Value.UseParameterized;
        // TODO 非参数化查询
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
                sql.Replace(item.Name, database.HandleBooleanValue((bool)item.Value));
            }
            else if (item.Type == ExpValueType.BooleanReverse)
            {
                sql.Replace(item.Name, database.HandleBooleanValue(!(bool)item.Value));
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
    protected void ResolveExpressions(ICustomDatabase database)
    {
        if (Expressions.Completed)
        {
            return;
        }
        ResolveCtx = new ResolveContext(database);
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
            HandleResult(database, item, result);
            DbParameterInfos.AddRange(result.DbParameters ?? []);
            index++;
        }
    }
    protected abstract void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result);

    public abstract string ToSqlString(ICustomDatabase database);

    public static string GetTableName(ICustomDatabase database, TableInfo ti, bool useAlias = true, bool useEmphasis = true)
    {
        return $"{NpTableName(database, ti)}{((useAlias && !string.IsNullOrEmpty(ti.Alias)) ? $" {ti.Alias}" : "")}";
    }



    //TODO Oracle?
    protected static string NpTableName(ICustomDatabase database, TableInfo table)
    {
        if (table.TableEntityInfo.IsTempTable)
        {
            return table.TableEntityInfo.TableName;
        }
        var tablename = table.TableName;
        if (!tablename.Contains('.'))
        {
            return database.AttachEmphasis(tablename);
        }
        else
        {
            var prs = tablename.Split('.');
            return string.Join(".", prs.Select(database.AttachEmphasis));
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

    ///// <summary>
    ///// 过滤SQL值以防止注入
    ///// </summary>
    //private static string SanitizeSqlValue(object value)
    //{
    //    if (value == null) return "NULL";

    //    switch (value)
    //    {
    //        case string s:
    //            // 转义单引号并包裹在引号中
    //            return "'" + s.Replace("'", "''") + "'";

    //        case DateTime dt:
    //            // 日期时间格式化
    //            return "'" + dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";

    //        case DateTimeOffset dto:
    //            return "'" + dto.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") + "'";

    //        case Guid guid:
    //            return "'" + guid.ToString() + "'";

    //        case byte[] bytes:
    //            // 二进制数据转为十六进制字符串
    //            return "0x" + BitConverter.ToString(bytes).Replace("-", "");

    //        case IFormattable formattable:
    //            // 数字类型直接ToString
    //            return formattable.ToString(null, CultureInfo.InvariantCulture);

    //        case IEnumerable enumerable when value is not string:
    //            // 处理集合类型
    //            var items = new List<string>();
    //            foreach (var item in enumerable)
    //            {
    //                items.Add(SanitizeSqlValue(item));
    //            }
    //            return "(" + string.Join(", ", items) + ")";

    //        default:
    //            // 其他类型直接ToString并转义
    //            return "'" + value.ToString().Replace("'", "''") + "'";
    //    }
    //}
}