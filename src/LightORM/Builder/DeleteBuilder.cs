using LightORM.Extension;
using System.Linq;
using System.Text;

namespace LightORM.Builder;
internal record DeleteBuilder(DbBaseType type) : SqlBuilder(type)
{
    public bool IsDeleteList { get; set; }
    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
        {
            Where.Add(result.SqlString!);
        }
    }
    public override string ToSqlString()
    {
        //TODO 处理批量删除
        ResolveExpressions();
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
        sql.AppendLine($"WHERE {string.Join(" AND ", Where)}");
        return sql.Trim();
    }


}
