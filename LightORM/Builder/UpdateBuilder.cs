using System.Linq;
using System.Text;

namespace LightORM.Builder;

internal class UpdateBuilder : SqlBuilder
{
    List<string> IgnoreMembers { get; set; } = [];
    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
        {
            Where.Add(result.SqlString!);
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Update)
        {
            Members.AddRange(result.Members!);
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
        }
    }
    public override string ToSqlString()
    {
        //TODO 处理批量更新
        ResolveExpressions();
        StringBuilder sb = new StringBuilder();
        if (Where.Count == 0)
        {
            var primaryCol = TableInfo.Columns.Where(c => c.IsPrimaryKey).ToArray();
            if (primaryCol.Length == 0) throw new LightOrmException("Where Condition is null and no primarykey");
            if (TargetObject == null) throw new LightOrmException("Where Condition is null and no entity");
            foreach (var item in primaryCol)
            {
                var val = item.GetValue(TargetObject);
                if (val == null) continue;
                DbParameters.Add(item.PropName, val);
                Where.Add($"{AttachEmphasis(item.ColumnName)} = {AttachPrefix(item.PropName)}");
            }
        }

        if (Members.Count == 0)
        {
            var autoUpdateCols = TableInfo.Columns
               .Where(c => !IgnoreMembers.Contains(c.PropName))
               .Where(c => !c.IsNotMapped)
               .Where(c => !c.IsPrimaryKey).ToArray();
            //参数处理
            foreach (var item in autoUpdateCols)
            {
                //参数中有对应属性的值，优先级更高
                if (DbParameters.ContainsKey(item.PropName))
                {
                    Members.Add(item.PropName);
                    continue;
                }
                else
                {
                    if (TargetObject != null)
                    {
                        var val = item.GetValue(TargetObject);
                        if (val == null) continue;
                        DbParameters.Add(item.PropName, val);
                        Members.Add(item.PropName);
                    }
                }
            }
        }

        var finalUpdateCol = TableInfo.Columns.Where(c => Members.Contains(c.PropName));

        sb.AppendFormat("UPDATE {0} SET\n{1}\n", GetTableName(TableInfo, false), string.Join(",\n", finalUpdateCol.Select(c => $"{AttachEmphasis(c.ColumnName)} = {AttachPrefix(c.PropName)}")));

        sb.AppendFormat("WHERE {0}", string.Join("\nAND ", Where));

        return sb.ToString();
    }


}
