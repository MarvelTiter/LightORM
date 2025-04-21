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
    public IEnumerable<T> TargetObjects { get; set; } = Enumerable.Empty<T>();
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    List<string> IgnoreMembers { get; set; } = [];
    public List<string> SetNullMembers { get; set; } = [];
    public bool IsBatchUpdate { get; internal set; }

    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
        {
            Where.Add(result.SqlString!);
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

        var primaryCol = MainTable.Columns.Where(c => c.IsPrimaryKey).ToArray();
        ITableColumnInfo[] columns = MainTable.Columns
                   .Where(c => !IgnoreMembers.Contains(c.PropertyName))
                   .Where(CheckMembers)
                   .Where(c => !c.IsNotMapped)
                   .Where(c => !c.IsNavigate).ToArray();

        BatchInfos = columns.GenBatchInfos(TargetObjects.ToList(), 2000 - DbParameters.Count);
        var update = $"UPDATE {GetTableName(MainTable, false)} SET";
        var primaryWhen = $"WHEN {string.Join(" AND ", primaryCol.Select(p => $"{AttachPrefix(p.ColumnName)}_{{0}}"))}";
        foreach (var batch in BatchInfos)
        {
            StringBuilder sb = new StringBuilder(update);
            foreach (var col in columns)
            {
                sb.Append($"\n{AttachEmphasis(col.ColumnName)} = CASE ");

                foreach (var rowDatas in batch.Parameters)
                {
                    var currentCol = rowDatas.First(r => r.PropName == col.PropertyName);
                    sb.Append($"WHEN {string.Join(" AND ", rowDatas.Where(r => r.IsPrimaryKey).Select(r => $"{AttachEmphasis(r.ColumnName)} = {AttachPrefix(r.ParameterName)}"))} THEN {(currentCol.Value == null ? "NULL" : AttachPrefix(currentCol.ParameterName))} ");
                }
                sb.Append("END,");
            }

            sb.Remove(sb.Length - 1, 1);

            var pValues = batch.Parameters.SelectMany(rowDatas => rowDatas.Where(r => r.IsPrimaryKey));

            foreach (var item in pValues.GroupBy(c => c.ColumnName))
            {
                Where.Add($"( {AttachEmphasis(item.Key)} IN ({string.Join(", ", item.Select(i => AttachPrefix(i.ParameterName)))}) )");
            }

            sb.AppendFormat("\nWHERE {0}", string.Join("\nAND ", Where));

            batch.Sql = sb.ToString();
        }
        batchDone = true;
    }

    public override string ToSqlString()
    {
        if (IsBatchUpdate)
        {
            CreateUpdateBatchSql();
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }
        ResolveExpressions();
        StringBuilder sb = new StringBuilder();
        if (Where.Count == 0)
        {
            var primaryCol = MainTable.Columns.Where(c => c.IsPrimaryKey).ToArray();
            if (primaryCol.Length == 0) LightOrmException.Throw("Where Condition is null and no primarykey");
            if (TargetObject == null) LightOrmException.Throw("Where Condition is null and no entity");
            foreach (var item in primaryCol)
            {
                var val = item.GetValue(TargetObject);
                if (val == null) continue;
                DbParameters.Add(item.PropertyName, val);
                Where.Add($"{AttachEmphasis(item.ColumnName)} = {AttachPrefix(item.PropertyName)}");
            }
        }

        if (Members.Count == 0)
        {
            var autoUpdateCols = MainTable.Columns
               .Where(c => !IgnoreMembers.Contains(c.PropertyName))
               .Where(c => !c.IsNotMapped)
               .Where(c => !c.IsNavigate)
               .Where(c => !c.IsPrimaryKey).ToArray();
            //参数处理
            foreach (var item in autoUpdateCols)
            {
                if (TargetObject != null)
                {
                    var val = item.GetValue(TargetObject);
                    if (val == null) continue;
                    DbParameters.Add(item.PropertyName, val);
                    Members.Add(item.PropertyName);
                }
            }
        }

        var customCols = MainTable.Columns.Where(c => Members.Contains(c.PropertyName));
        var finalUpdateCol = customCols.Select(c => $"{AttachEmphasis(c.ColumnName)} = {AttachPrefix(c.PropertyName)}");

        var setNullCol = MainTable.Columns.Where(c => SetNullMembers.Contains(c.PropertyName)).ToArray();
        if (setNullCol.Length > 0)
        {
            finalUpdateCol = finalUpdateCol.Concat(setNullCol.Select(c => $"{AttachEmphasis(c.ColumnName)} = NULL"));
        }

        sb.AppendFormat("UPDATE {0} SET\n{1}", GetTableName(MainTable, false), string.Join(",\n", finalUpdateCol));

        sb.AppendFormat("\nWHERE {0}", string.Join("\nAND ", Where));

        return sb.ToString();
    }


}
