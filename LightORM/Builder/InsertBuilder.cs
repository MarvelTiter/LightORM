using System.Linq;
using System.Text;

namespace LightORM.Builder;

internal class InsertBuilder : SqlBuilder
{
    public List<string> IgnoreMembers { get; set; } = [];
    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Insert)
        {
            Members.AddRange(result.Members!);
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
        }
    }
    public bool IsInsertList { get; set; }
    public override string ToSqlString()
    {
        //TODO 处理批量插入
        if (TargetObject == null) throw new LightOrmException("insert null entity");
        ResolveExpressions();
        StringBuilder sb = new StringBuilder();
        if (Members.Count == 0)
        {
            Members.AddRange(TableInfo.Columns.Where(c => !c.IsNavigate && !c.IsNotMapped).Select(c => c.PropName));
        }
        var insertColumns = TableInfo.Columns
            .Where(c => c.GetValue(TargetObject) != null)
            .Where(c => !IgnoreMembers.Contains(c.PropName))
            .Where(c => Members.Contains(c.PropName) && !c.IsNotMapped && !c.IsNavigate).ToArray();
        foreach (var item in insertColumns)
        {
            var val = item.GetValue(TargetObject);
            DbParameters.Add(item.PropName, val!);
        }
        sb.AppendFormat("INSERT INTO {0} \n({1}) \nVALUES \n({2})"
            , GetTableName(TableInfo, false)
            , string.Join(", ", insertColumns.Select(c => AttachEmphasis(c.ColumnName)))
            , string.Join(", ", insertColumns.Select(c => AttachPrefix(c.PropName))));
        return sb.ToString();
    }


}