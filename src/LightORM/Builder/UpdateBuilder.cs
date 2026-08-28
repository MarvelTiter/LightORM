using LightORM.Extension;
using System.Collections.Concurrent;
using System.Text;

namespace LightORM.Builder;

//internal readonly record struct BadValue
//{
//    public static readonly BadValue Instance = new();
//}

internal record struct VersionInfo
{
    public string? PropertyName { get; set; }
    public object? VersionValue { get; set; }
}

internal class UpdateBuilder<T> : SqlBuilder
{
    private static readonly ConcurrentDictionary<string, ITableColumnInfo> columnCaches = [];

    private static ITableColumnInfo GetColumn(TableInfo tableInfo, string propertyName)
    {
        if (!columnCaches.TryGetValue(propertyName, out var columnInfo))
        {
            columnInfo = tableInfo.GetColumnInfo(propertyName);
            columnCaches[propertyName] = columnInfo;
        }
        return columnInfo;
    }

    //public new T? TargetObject { get; set; }
    public T[] TargetObjects { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    HashSet<string> IgnoreMembers { get; set; } = [];
    HashSet<string> Members { get; set; } = [];
    HashSet<string> SetNullMembers { get; set; } = [];
    HashSet<string> WhereMembers { get; set; } = [];
    Dictionary<string, string?> UpdateSpecific { get; set; } = [];
    public bool IsBatchUpdate { get; internal set; }
    public VersionInfo VersionInfo { get; set; }
    public bool UseVersionColumn { get; private set; }
    public void AddMember(string member, object? value)
    {
        Members.Add(member);
        if (value is not null)
        {
            DbParameters.TryAdd(member, value);
        }
    }
    protected override void HandleResult(IDatabaseAdapter database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions.SqlType == SqlPartial.Where)
        {
            Where.Add(result.SqlString!);
            WhereMembers.AddRange(result.Members);
        }
        else if (expInfo.ResolveOptions.SqlType == SqlPartial.Update)
        {
            /*
             * AdditionalParameter为SpecificValue的情况分别是调用了下面这两个
             * IExpUpdate<T> SetNull<TNull>(Expression<Func<T, TNull>> exp) => 指定设置为Null的字段，可单个，可多个
             * IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value) => 使用value设置指定单个字段的值
             * 
             * AdditionalParameter为Null的情况
             * IExpUpdate<T> Set(Expression<Func<T, bool>> exp) => 使用BinaryExpression设置单个字段的值
             * IExpUpdate<T> UpdateColumns<TUpdate>(Expression<Func<T, TUpdate>> columns) => 指定更新的字段，可单个，可多个
             */
            if (result.Members?.Count > 0)
            {
                var propertyName = result.Members[0];
                var col = GetColumn(MainTable, propertyName);
                if (expInfo.AdditionalParameter is SpecificValue v)
                {
                    if (v.Value is null)
                    {
                        SetNullMembers.AddRange(result.Members);
                    }
                    else
                    {
                        if (col.IsJsonColumn)
                        {
                            UpdateSpecific.Add(col.PropertyName, result.SqlString);
                        }
                        else
                        {
                            UpdateSpecific.Add(col.PropertyName, $"{result.SqlString} = {database.AttachPrefix(col.PropertyName)}");
                        }
                        DbParameters.Add(col.PropertyName, v.Value);
                        Members.Add(col.PropertyName);
                    }
                }
                else
                {
                    if (col.IsJsonColumn || result.SqlString?.Contains('=') == true)
                    {
                        UpdateSpecific.Add(col.PropertyName, result.SqlString);
                        //DbParameters.Add(col.PropertyName, BadValue.Instance);
                    }
                    Members.AddRange(result.Members);
                }
            }
            else
            {
                throw new LightOrmException("未解析到属性");
            }
            //if (expInfo.AdditionalParameter is null)
            //{
            //    // 从UpdateProvider的代码来看，使用SqlFn.JsonSet或者调用SetNull，一定是进入这个分支
            //    if (result.Members?.Count == 1 && result.SqlString is not null)
            //    {
            //        // TODO 需要优化
            //        var col = GetColumn(MainTable, result.Members[0]);
            //        if (col.IsJsonColumn)
            //        {
            //            UpdateSpecific.Add(result.Members[0], result.SqlString);
            //            DbParameters.Add(result.Members[0], BadValue.Instance);
            //        }
            //    }
            //    Members.AddRange(result.Members);
            //}
            //else if (expInfo.AdditionalParameter is SpecificValue v)
            //{
            //    if (v.Value is null)
            //    {
            //        SetNullMembers.AddRange(result.Members);
            //    }
            //    else
            //    {
            //        if (result.Members?.Count > 1)
            //        {
            //            var last = result.Members[result.Members.Count - 1];
            //            var col = MainTable.GetColumnInfo(last);
            //            if (col.IsJsonColumn)
            //            {
            //                UpdateSpecific.Add(col.PropertyName, result.SqlString);
            //                DbParameters.Add(col.PropertyName, v.Value);
            //                Members.Add(last);
            //                return;
            //            }
            //        }
            //        var member = result.Members![0];
            //        Members.Add(member);
            //        DbParameters.Add(member, v.Value);
            //    }
            //}
        }
        else if (expInfo.ResolveOptions.SqlType == SqlPartial.UpdateVersionColumn)
        {
            if (result.Members?.Count == 1 && expInfo.AdditionalParameter is SpecificValue sv && sv.Value is not null)
            {
                var name = result.Members[0];
                VersionInfo = new() { PropertyName = name, VersionValue = sv.Value };
            }
        }
        else if (expInfo.ResolveOptions.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
            //IgnoreMembers = new(result.Members!)
        }
    }
    bool batchDone = false;
    //bool CheckMembers(ITableColumnInfo col)
    //{

    //}
    private void CreateUpdateBatchSql(IDatabaseAdapter database)
    {
        if (batchDone)
        {
            return;
        }

        // TODO 批量更新对JSON列处理

        var columns = MainTable.TableEntityInfo.Columns
                   .Where(col =>
                   {
                       if (col.IsJsonColumn)
                       {
                           return false;
                       }
                       // 如果 IgnoreMembers 非空，排除被忽略的列
                       if (IgnoreMembers.Count > 0 && IgnoreMembers.Contains(col.PropertyName))
                           return false;

                       // 如果 Members 为空，且当前列不是被强制保留的类型，则需进一步判断
                       if (Members.Count == 0)
                       {
                           // 当 Members 为空时，只要没被 Ignore 就保留
                           return true;
                       }

                       // Members 非空：只保留显式指定的成员，主键、版本号等始终保留
                       if (col.IsPrimaryKey || col.IsVersionColumn)
                           return true;

                       // 自增列、未映射、导航属性、聚合属性、禁止更新的列，始终不更新
                       if (col.AutoIncrement || col.IsNotMapped || col.IsNavigate || col.IsAggregated || col.IsIgnoreUpdate)
                           return false;

                       // 最终：是否在 Members 中
                       return Members.Contains(col.PropertyName);
                   })
                   .ToArray();

        BatchInfos = columns.GenBatchInfos(TargetObjects, database, 2000 - DbParameters.Count, DbParameters);

        database.HandleBatchUpdate<T>(new(this, columns, BatchInfos, DbParameters, database));

        batchDone = true;

    }

    public override string ToSqlString(IDatabaseAdapter database)
    {
        if (QuoteIdentifiers.HasValue)
        {
            database = new ScopedDatabaseAdapter(database, QuoteIdentifiers.Value);
        }
        ResolveExpressions(database);
        if (IsBatchUpdate)
        {
            CreateUpdateBatchSql(database);
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }
        if (Where.Count == 0)
        {
            var primaryCol = MainTable.TableEntityInfo.Columns.Where(c => c.IsPrimaryKey).ToArray();
            if (primaryCol.Length == 0)
            {
                throw new LightOrmException($"Where Condition is null and Model of [{MainTable.Type}] do not has a PrimaryKey");
            }
            if (TargetObject == null)
            {
                throw new LightOrmException("Where Condition is null and no entity");
            }
            foreach (var item in primaryCol)
            {
                var val = item.GetValue(TargetObject!);
                if (val == null) continue;
                DbParameters.Add(item.PropertyName, val);
                Where.Add($"({database.AttachEmphasis(item.ColumnName)} = {database.AttachPrefix(item.PropertyName)})");
                //WhereMembers.Add(item.PropertyName);
            }
        }

        if (Members.Count == 0)
        {
            var autoUpdateCols = MainTable.TableEntityInfo.Columns
               .Where(c =>
               {
                   if (IgnoreMembers.Count > 0 && IgnoreMembers.Contains(c.PropertyName))
                   {
                       return false;
                   }
                   if (c.IsNotMapped || c.IsNavigate || c.IsPrimaryKey || c.IsAggregated || c.IsVersionColumn || c.IsIgnoreUpdate || c.AutoIncrement)
                   {
                       return false;
                   }
                   return true;
               });
            //参数处理
            foreach (var item in autoUpdateCols)
            {
                if (TargetObject is not null)
                {
                    var val = item.GetValue(TargetObject);
                    if (val == null) continue;
                    DbParameters.Add(item.PropertyName, val);
                    Members.Add(item.PropertyName);
                }
            }
        }

        var customCols = MainTable.TableEntityInfo.Columns.Where(c => Members.Contains(c.PropertyName) && !SetNullMembers.Contains(c.PropertyName));

        var setNullCol = MainTable.TableEntityInfo.Columns.Where(c => SetNullMembers.Count > 0 && SetNullMembers.Contains(c.PropertyName)).ToList();
        using var _ = StringBuilderPool.Get(out var sb);
        //StringBuilder sb = new("UPDATE ");
        WriteTags(sb);
        sb.Append("UPDATE ");
        sb.AppendTableName(database, MainTable, false);
        sb.AppendLine(" SET   ");
        bool valueFounded;
        foreach (var c in customCols)
        {
            if (UpdateSpecific.TryGetValue(c.PropertyName, out var fieldSql))
            {
                sb.Append(fieldSql);
                if (c.IsJsonColumn)
                {
                    database.HandleJsonParameter(new(ActionType.ParameterValue, c, sb, null, DbParameters, ExpressionSqlOptions.Instance.Value.GetJsonHandler()));
                }
                sb.AppendLine(",");
                continue;
            }
            // 处理一般列
            valueFounded = DbParameters.TryGetValue(c.PropertyName, out var value);
            if (!valueFounded)
            {
                if (TargetObject is not null)
                    value = c.GetValue(TargetObject);
                if (value is null)
                {
                    continue;
                }
            }
            sb.AppendEmphasis(c.ColumnName, database);
            sb.Append(" = ");
            sb.WithPrefix(c.PropertyName, database);
            if (c.IsJsonColumn)
            {
                // TODO 暂时做法，兼容postgresql，在后面追加::JSONB
                database.HandleJsonParameter(new(ActionType.Parameterized, c, sb, null, DbParameters, ExpressionSqlOptions.Instance.Value.GetJsonHandler()));
                var jsonHandler = ExpressionSqlOptions.Instance.Value.GetJsonHandler();
                var jsonString = jsonHandler.Serialize(value);
                DbParameters[c.PropertyName] = jsonString;
            }
            if (!valueFounded)
            {
                DbParameters.Add(c.PropertyName, value!);
            }

            sb.AppendLine(",");
        }
        foreach (var c in setNullCol)
        {
            // 处理显式设置为Null值的列
            //sb.AppendLine($"{database.AttachEmphasis(c.ColumnName)} = NULL,");
            sb.AppendEmphasis(c.ColumnName, database);
            sb.AppendLine(" = NULL,");
        }
        HandleVersionColumn(sb, database);

        sb.RemoveLast(N.Length + 1);
        sb.AppendLine();
        //sb.AppendLine($"WHERE {string.Join(" AND ", Where)}");
        sb.Append("WHERE ");
        sb.AppendJoined(Where, " AND ");
        HandleSqlParameters(sb, database);
        return sb.Trim();
    }


    private void HandleVersionColumn(StringBuilder sb, IDatabaseAdapter database)
    {
        if (TargetObject is null && VersionInfo == default)
        {
            return;
        }
        ITableColumnInfo? versionColumn = null;
        if (VersionInfo.PropertyName is not null)
        {
            versionColumn = GetColumn(MainTable, VersionInfo.PropertyName);
        }
        else
        {
            versionColumn = MainTable.TableEntityInfo.Columns.FirstOrDefault(c => c.IsVersionColumn);
        }
        if (versionColumn is null)
        {
            return;
        }
        // 使用了实体更新，或者直接使用了WithVersion设置版本列
        UseVersionColumn = (TargetObject is not null) || (VersionInfo.VersionValue is not null);
        if (WhereMembers.Contains(versionColumn.PropertyName))
        {
            throw new LightOrmException($"请勿在Where条件添加Version列({versionColumn.PropertyName})的条件判断，如有必要，请使用WithVersion方法");
        }
        var oldVersion = GetOldVersionValue();
        var newVersion = VersionPlus(oldVersion);
        DbParameters.Add($"{versionColumn.PropertyName}_n", newVersion);
        DbParameters.TryAdd(versionColumn.PropertyName, oldVersion);

        // 处理版本列
        sb.AppendEmphasis(versionColumn.ColumnName, database);
        sb.Append(" = ");
        sb.WithPrefix($"{versionColumn.PropertyName}_n", database);
        sb.AppendLine(",");

        Where.Add($"({database.AttachEmphasis(versionColumn.ColumnName)} = {database.AttachPrefix($"{versionColumn.PropertyName}")})");

        object GetOldVersionValue()
        {
            if (VersionInfo.VersionValue is not null)
            {
                return VersionInfo.VersionValue;
            }
            if (TargetObject is not null)
            {
                return versionColumn.GetValue(TargetObject)!;
            }
            throw new LightOrmException("使用了实体更新，或者直接使用了WithVersion设置版本列，但是未提供版本值");
        }
    }
}
