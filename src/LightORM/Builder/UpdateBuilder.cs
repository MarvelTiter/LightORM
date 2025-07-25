using LightORM.Extension;
using System;
using System.Linq;
using System.Text;

namespace LightORM.Builder;

internal struct UpdateValue
{
    public object? Value { get; set; }
}

internal record UpdateBuilder<T>(DbBaseType type) : SqlBuilder(type)
{
    public new T? TargetObject { get; set; }
    public IEnumerable<T> TargetObjects { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    List<string> IgnoreMembers { get; set; } = [];
    public List<string> SetNullMembers { get; set; } = [];
    public bool IsBatchUpdate { get; internal set; }
    public List<string> WhereMembers { get; set; } = [];

    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
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
                var member = result.Members!.First();
                if (v.Value is null)
                {
                    SetNullMembers.Add(member);
                }
                else
                {
                    Members.Add(member);
                    DbParameters.Add(member, v.Value);
                }
            }
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
        }
    }
    bool batchDone = false;
    bool CheckMembers(ITableColumnInfo col)
    {
        if (Members.Count == 0) return true;
        return Members.Contains(col.PropertyName);
    }
    private void CreateUpdateBatchSql()
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions();

        //var primaryCol = MainTable.TableEntityInfo.Columns.Where(c => c.IsPrimaryKey).ToArray();
        // 筛选需要的列
        var columns = MainTable.TableEntityInfo.Columns
                   .Where(c => !IgnoreMembers.Contains(c.PropertyName))
                   .Where(CheckMembers)
                   .Where(c => !c.IsNotMapped && !c.IsNavigate && !c.IsAggregated).ToArray();

        BatchInfos = columns.GenBatchInfos(TargetObjects.ToList(), 2000 - DbParameters.Count);
        var update = $"UPDATE {GetTableName(MainTable, false)} SET";
        //var primaryWhen = $"WHEN {string.Join(" AND ", primaryCol.Select(p => $"{AttachPrefix(p.ColumnName)}_{{0}}"))}";
        foreach (var batch in BatchInfos)
        {
            // 每一个BatchSqInfo就是每批次更新的数据量
            StringBuilder sb = new(update);
            foreach (var col in columns)
            {
                if (col.IsPrimaryKey) continue;
                sb.Append($"\n{AttachEmphasis(col.ColumnName)} = CASE ");

                // 每一条记录的参数数量
                foreach (var rowDatas in batch.Parameters)
                {
                    var currentCol = rowDatas.First(r => r.PropName == col.PropertyName);
                    if (currentCol.IsVersion)
                    {
                        var newVersion = VersionPlus(currentCol.Value);
                        var newCol = currentCol with { ParameterName = $"{currentCol.ParameterName}_new", Value = newVersion };
                        sb.Append($"WHEN {string.Join(" AND ", rowDatas.Where(r => r.IsPrimaryKey || r.IsVersion).Select(r => $"{AttachEmphasis(r.ColumnName)} = {AttachPrefix(r.ParameterName)}"))} THEN {(newCol.Value == null ? "NULL" : AttachPrefix(newCol.ParameterName))} ");
                        rowDatas.Add(newCol);
                    }
                    else
                    {
                        sb.Append($"WHEN {string.Join(" AND ", rowDatas.Where(r => r.IsPrimaryKey || r.IsVersion).Select(r => $"{AttachEmphasis(r.ColumnName)} = {AttachPrefix(r.ParameterName)}"))} THEN {(currentCol.Value == null ? "NULL" : AttachPrefix(currentCol.ParameterName))} ");
                    }
                }
                sb.Append("END,");
            }

            sb.RemoveLast(1);

            var pValues = batch.Parameters.SelectMany(rowDatas => rowDatas.Where(r => r.IsPrimaryKey));

            foreach (var item in pValues.GroupBy(c => c.ColumnName))
            {
                Where.Add($"( {AttachEmphasis(item.Key)} IN ({string.Join(", ", item.Select(i => AttachPrefix(i.ParameterName)))}))");
            }

            sb.AppendLine($"WHERE {string.Join($"{N}AND ", Where)}");
            HandleSqlParameters(sb);
            batch.Sql = sb.ToString();
        }
        batchDone = true;
    }

    public override string ToSqlString()
    {
        ResolveExpressions();
        if (IsBatchUpdate)
        {
            CreateUpdateBatchSql();
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
                Where.Add($"({AttachEmphasis(item.ColumnName)} = {AttachPrefix(item.PropertyName)})");
                WhereMembers.Add(item.PropertyName);
            }
        }

        if (Members.Count == 0)
        {
            var autoUpdateCols = MainTable.TableEntityInfo.Columns
               .Where(c => !IgnoreMembers.Contains(c.PropertyName))
               .Where(c => !c.IsNotMapped && !c.IsNavigate && !c.IsPrimaryKey && !c.IsAggregated && !c.IsVersionColumn && !c.IsIgnoreUpdate).ToList();
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
                DbParameters.Add($"{versionColumn.PropertyName}_new", newVersion);
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
        IEnumerable<string>? finalUpdateCol = customCols.Select(c => $"{AttachEmphasis(c.ColumnName)} = {AttachPrefix(c.PropertyName)}");

        var setNullCol = MainTable.TableEntityInfo.Columns.Where(c => SetNullMembers.Contains(c.PropertyName)).ToArray();
        if (setNullCol.Length > 0)
        {
            finalUpdateCol = finalUpdateCol.Concat(setNullCol.Select(c => $"{AttachEmphasis(c.ColumnName)} = NULL"));
        }
        if (versionColumn is not null)
        {
            finalUpdateCol = finalUpdateCol.Concat([$"{AttachEmphasis(versionColumn.ColumnName)} = {AttachPrefix($"{versionColumn.PropertyName}_new")}"]);
            if (!WhereMembers.Contains(versionColumn.PropertyName))
            {
                var versonCondition = $"({AttachEmphasis(versionColumn.ColumnName)} = {AttachPrefix($"{versionColumn.PropertyName}")})";
                if (!string.IsNullOrEmpty(versonCondition))
                    Where.Add(versonCondition);
            }
        }
        StringBuilder sb = new("UPDATE ");
        sb.Append(GetTableName(MainTable, false));
        sb.AppendLine(" SET");
        sb.AppendLine(string.Join($",{N}", finalUpdateCol));
        sb.AppendLine($"WHERE {string.Join(" AND ", Where)}");
        HandleSqlParameters(sb);
        return sb.Trim();
    }
}
