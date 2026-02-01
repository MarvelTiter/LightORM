using LightORM.Extension;
using System.Text;

namespace LightORM.Builder;

internal struct UpdateValue
{
    public object? Value { get; set; }
}

internal record UpdateBuilder<T> : SqlBuilder
{
    public new T? TargetObject { get; set; }
    public T[] TargetObjects { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    HashSet<string> IgnoreMembers { get; set; } = [];
    HashSet<string> Members { get; set; } = [];
    HashSet<string> SetNullMembers { get; set; } = [];
    HashSet<string> WhereMembers { get; set; } = [];
    public bool IsBatchUpdate { get; internal set; }

    protected override void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
        {
            Where.Add(result.SqlString!);
            WhereMembers.AddRange(result?.Members?.Distinct() ?? []);
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Update)
        {
            if (expInfo.AdditionalParameter is null)
            {
                Members.AddRange(result.Members!);
            }
            else if (expInfo.AdditionalParameter is UpdateValue v)
            {
                if (v.Value is null)
                {
                    SetNullMembers.AddRange(result.Members!);
                }
                else
                {
                    var member = result.Members!.First();
                    Members.Add(member);
                    DbParameters.Add(member, v.Value);
                }
            }
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
            //IgnoreMembers = new(result.Members!)
        }
    }
    bool batchDone = false;
    //bool CheckMembers(ITableColumnInfo col)
    //{

    //}
    private void CreateUpdateBatchSql(ICustomDatabase database)
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions(database);

        //var primaryCol = MainTable.TableEntityInfo.Columns.Where(c => c.IsPrimaryKey).ToArray();
        // 筛选需要的列

        var columns = MainTable.TableEntityInfo.Columns
                   .Where(col =>
                   {
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

        BatchInfos = columns.GenBatchInfos(TargetObjects, 2000 - DbParameters.Count, DbParameters);
        //var update = $"UPDATE {GetTableName(database, MainTable, false)} SET";
        //var primaryWhen = $"WHEN {string.Join(" AND ", primaryCol.Select(p => $"{AttachPrefix(p.ColumnName)}_{{0}}"))}";
        foreach (var batch in BatchInfos)
        {
            // 每一个BatchSqInfo就是每批次更新的数据量
            StringBuilder sb = new("UPDATE ");
            //sb.Append(GetTableName(database, MainTable, false));
            sb.AppendTableName(database, MainTable, false);
            sb.Append(" SET ");
            for (int i = 0; i < columns.Length; i++)
            {
                ITableColumnInfo? col = columns[i];
                if (col.IsPrimaryKey) continue;
                //sb.Append($"\n{database.AttachEmphasis(col.ColumnName)} = CASE ");
                sb.AppendEmphasis(col.ColumnName, database);
                sb.AppendLine(" = CASE");
                // 每一条记录的参数数量
                for (var rowIndex = 0; rowIndex < batch.Parameters.Count; rowIndex++)
                {
                    var rowDatas = batch.Parameters[rowIndex];
                    var currentCol = rowDatas.First(r => r.PropName == col.PropertyName);
                    if (currentCol.IsVersion)
                    {
                        var newVersion = VersionPlus(currentCol.Value);
                        var newCol = currentCol with { ParameterName = $"{currentCol.ParameterName}_n", Value = newVersion, IsVersion = false };
                        rowDatas.Add(newCol);
                        currentCol = newCol;
                    }
                    bool first = true;
                    sb.Append("  WHEN ");
                    foreach (var item in rowDatas.Where(r => r.IsPrimaryKey || r.IsVersion))
                    {
                        if (!first) sb.Append(" AND ");
                        first = false;
                        sb.AppendEmphasis(item.ColumnName, database);
                        sb.Append(" = ");
                        sb.WithPrefix(item.ParameterName, database);
                    }
                    sb.Append(" THEN ");
                    sb.AppendLine(GetValueExpression(currentCol));
                    //if (currentCol.IsVersion)
                    //{
                    //    rowDatas.Add(currentCol);
                    //}
                }

                sb.Append("END, ");
            }

            sb.RemoveLast(2);

            var pValues = batch.Parameters.SelectMany(rowDatas => rowDatas.Where(r => r.IsPrimaryKey | r.IsVersion)).GroupBy(c => c.ColumnName).ToList();
            if (pValues.Count == 0 && Where.Count == 0)
            {
                throw new LightOrmException($"类型{typeof(T)}, 没有主键并且缺失Where条件");
            }
            sb.AppendLine();
            sb.Append("WHERE ");
            for (int k = 0; k < pValues.Count; k++)
            {
                IGrouping<string, SimpleColumn>? item = pValues[k];
                if (k > 0)
                {
                    sb.AppendLine();
                    sb.Append("AND ");
                }
                sb.Append('(');
                sb.AppendEmphasis(item.Key, database);
                sb.Append(" IN (");
                foreach (var i in item)
                {
                    sb.WithPrefix(i.ParameterName, database);
                    sb.Append(',');
                }
                sb.RemoveLast(1);
                sb.Append("))");
            }
            if (Where.Count > 0)
            {
                //if (pValues.Count == 0)
                for (int i = 0; i < Where.Count; i++)
                {
                    if (i > 0 || pValues.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append("AND ");
                    }
                    sb.Append(Where[i]);
                }
            }
            HandleSqlParameters(sb, database);
            batch.Sql = sb.ToString();
        }
        batchDone = true;

        string GetValueExpression(SimpleColumn col)
        {
            if (col.Value is null)
            {
                return "NULL";
            }
            if (col.isStaticValue)
            {
                return FormatStaticValue(col.Value);
            }
            return database.AttachPrefix(col.ParameterName);
        }
        string FormatStaticValue(object value)
        {
            return value switch
            {
                // 字符串：用单引号包裹，并转义单引号（基础防护）
                string s => $"'{s.Replace("'", "''")}'",

                // 布尔值：根据 SQL 标准，多数数据库用 1/0，PostgreSQL 用 true/false
                bool b => database.HandleBooleanValue(b),

                // 整数类型
                sbyte or byte or short or ushort or int or uint or long or ulong => $"{value}",

                // 浮点数
                float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
                double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture),

                // 日期时间（可选支持）
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
#if NET6_0_OR_GREATER
                DateOnly dOnly => $"'{dOnly:yyyy-MM-dd}'",
                TimeOnly tOnly => $"'{tOnly:HH:mm:ss}'",
#endif
                // Guid（可选）
                Guid g => $"'{g}'",

                // 不支持的类型：抛出异常或返回 NULL / 占位符
                _ => throw new NotSupportedException($"Static value of type '{value.GetType()}' is not supported in SQL literal generation.")
            };
        }
    }

    public override string ToSqlString(ICustomDatabase database)
    {
        ResolveExpressions(database);
        if (IsBatchUpdate)
        {
            CreateUpdateBatchSql(database);
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }
        if (Where.Count == 0)
        {
            var primaryCol = MainTable.TableEntityInfo.Columns.Where(c => c.IsPrimaryKey || c.IsVersionColumn).ToArray();
            if (primaryCol.Length == 0) LightOrmException.Throw("Where Condition is null and no primarykey");
            if (TargetObject == null) LightOrmException.Throw("Where Condition is null and no entity");
            foreach (var item in primaryCol)
            {
                var val = item.GetValue(TargetObject!);
                if (val == null) continue;
                DbParameters.Add(item.PropertyName, val);
                Where.Add($"({database.AttachEmphasis(item.ColumnName)} = {database.AttachPrefix(item.PropertyName)})");
                WhereMembers.Add(item.PropertyName);
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
        var versionColumn = MainTable.TableEntityInfo.Columns.Where(c => c.IsVersionColumn).FirstOrDefault();
        if (versionColumn is not null)
        {
            if (TargetObject is not null)
            {
                var version = versionColumn.GetValue(TargetObject);
                var newVersion = VersionPlus(version);
                DbParameters.Add($"{versionColumn.PropertyName}_n", newVersion);
#if NET462
                if (!DbParameters.ContainsKey(versionColumn.PropertyName))
                {
                    DbParameters.Add(versionColumn.PropertyName, version!);
                }
#else
                DbParameters.TryAdd(versionColumn.PropertyName, version!);
#endif
            }
        }
        var customCols = MainTable.TableEntityInfo.Columns.Where(c => Members.Contains(c.PropertyName) && !SetNullMembers.Contains(c.PropertyName));

        var setNullCol = MainTable.TableEntityInfo.Columns.Where(c => SetNullMembers.Count > 0 && SetNullMembers.Contains(c.PropertyName));

        StringBuilder sb = new("UPDATE ");
        sb.AppendTableName(database, MainTable, false);
        sb.AppendLine(" SET   ");
        foreach (var c in customCols)
        {
            // 处理一般列
            sb.AppendEmphasis(c.ColumnName, database);
            sb.Append(" = ");
            sb.WithPrefix(c.PropertyName, database);
            sb.AppendLine(",");
            //sb.AppendLine($"{database.AttachEmphasis(c.ColumnName)} = {database.AttachPrefix(c.PropertyName)},");
        }
        foreach (var c in setNullCol)
        {
            // 处理显式设置为Null值的列
            //sb.AppendLine($"{database.AttachEmphasis(c.ColumnName)} = NULL,");
            sb.AppendEmphasis(c.ColumnName, database);
            sb.AppendLine(" = NULL,");
        }
        if (versionColumn is not null)
        {
            // 处理版本列
            sb.AppendEmphasis(versionColumn.ColumnName, database);
            sb.Append(" = ");
            sb.WithPrefix($"{versionColumn.PropertyName}_n", database);
            sb.AppendLine(",");
            //sb.AppendLine($"{database.AttachEmphasis(versionColumn.ColumnName)} = {database.AttachPrefix($"{versionColumn.PropertyName}_new")},");
            if (!WhereMembers.Contains(versionColumn.PropertyName))
            {
                var versonCondition = $"({database.AttachEmphasis(versionColumn.ColumnName)} = {database.AttachPrefix($"{versionColumn.PropertyName}")})";
                if (!string.IsNullOrEmpty(versonCondition))
                    Where.Add(versonCondition);
            }
        }
        sb.RemoveLast(N.Length + 1);
        sb.AppendLine();
        sb.AppendLine($"WHERE {string.Join(" AND ", Where)}");
        HandleSqlParameters(sb, database);
        return sb.Trim();
    }
}
