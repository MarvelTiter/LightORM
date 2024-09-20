using System.Linq;
using System.Text;

namespace LightORM.Builder;
internal class DeleteBuilder : SqlBuilder
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
        StringBuilder sql = new StringBuilder();
        sql.AppendFormat("DELETE FROM {0}\n", GetTableName(MainTable, false));
        if (Where.Count == 0)
        {
            if (TargetObject == null)
            {
                throw new LightOrmException("Where Condition is null and not provider a entity value");
            }
            var primary = MainTable.Columns.Where(f => f.IsPrimaryKey).ToArray();
            if (primary.Length == 0) throw new LightOrmException($"Where Condition is null and Model of [{MainTable.Type}] do not has a PrimaryKey");
            var wheres = primary.Select(c =>
             {
                 DbParameters.Add(c.ColumnName, c.GetValue(TargetObject)!);
                 return $"{AttachEmphasis(c.ColumnName)} = {AttachPrefix(c.ColumnName)}";
             });
            Where.AddRange(wheres);
        }

        sql.AppendFormat("WHERE {0}", string.Join("\nAND ", Where));

        return sql.ToString();
    }


}
