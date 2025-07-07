using LightORM.Extension;
using System.Linq;
using System.Text;

namespace LightORM.Builder;
internal record DeleteBuilder<T>(DbBaseType type) : SqlBuilder(type)
{
    public new T? TargetObject { get; set; }
    public IEnumerable<T> TargetObjects { get; set; } = [];
    private bool batchDone = false;
    public bool IsBatchDelete { get; set; }
    public bool ForceDelete { get; set; }
    public bool Truncate { get; set; }
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
        {
            Where.Add(result.SqlString!);
        }
    }
    private void CreateBatchDeleteSql()
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions();
        var columns = MainTable.TableEntityInfo.Columns
                   .Where(c => c.IsPrimaryKey || c.IsVersionColumn).ToArray();

        BatchInfos = columns.GenBatchInfos(TargetObjects.ToList(), 2000 - DbParameters.Count);
        var delete = $"DELETE FROM {GetTableName(MainTable, false)}";
        foreach (var batch in BatchInfos)
        {
            StringBuilder sb = new();
            sb.AppendLine(delete);
            List<string> autoWhereList = [];
            foreach (var p in batch.Parameters)
            {
                var where = string.Join(" AND ", p.Select(c => $"{c.ColumnName} = {c.ParameterName}"));
                autoWhereList.Add($"({where})");
            }
            var autoWhere = string.Join(" OR ", autoWhereList);
            Where.Add(autoWhere);
            sb.AppendLine($"WHERE {string.Join($"{N}AND ", Where)}");
            batch.Sql = sb.ToString();
        }
        batchDone = true;
    }
    public override string ToSqlString()
    {
        //TODO 处理批量删除
        if (IsBatchDelete)
        {
            CreateBatchDeleteSql();
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }
        ResolveExpressions();
        if (ForceDelete)
        {
            if (Truncate)
            {
                return $"TRUNCATE TABLE {GetTableName(MainTable, false)}";
            }
            else
            {
                return $"DELETE FROM {GetTableName(MainTable, false)}";
            }
        }
        else
        {
            // 没有设置Where条件, 且提供实体值, 则使用主键作为Where条件
            if (Where.Count == 0)
            {
                if (TargetObject is null) LightOrmException.Throw("Where Condition is null and not provider a entity value");
                var primary = MainTable.TableEntityInfo.Columns.Where(f => f.IsPrimaryKey).ToArray();
                if (primary.Length == 0) LightOrmException.Throw($"Where Condition is null and Model of [{MainTable.Type}] do not has a PrimaryKey");
                var wheres = primary.Select(c =>
                 {
                     DbParameters.Add(c.ColumnName, c.GetValue(TargetObject!)!);
                     return $"{AttachEmphasis(c.ColumnName)} = {AttachPrefix(c.ColumnName)}";
                 });
                Where.AddRange(wheres);
            }
            StringBuilder sql = new("DELETE FROM ");
            sql.AppendLine(GetTableName(MainTable, false));
            if (Where.Count > 0)
            {
                sql.AppendLine($"WHERE {string.Join(" AND ", Where)}");
            }
            return sql.Trim();
        }
    }


}
