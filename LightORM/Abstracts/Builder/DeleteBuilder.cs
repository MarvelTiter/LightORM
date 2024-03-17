using System.Linq;
using System.Text;

namespace LightORM.Abstracts.Builder;

internal class DeleteBuilder: SqlBuilder
{
    public override string ToSqlString()
    {
        StringBuilder sql = new StringBuilder();
        var primary = TableInfo.Columns.Where(f => f.IsPrimaryKey).ToArray();
        bool deleteKey = Where.Count == 0;
        sql.AppendFormat("DELETE FROM {0} ", GetTableName(TableInfo,false));
        if (deleteKey)
        {
            if (!primary.Any()) throw new InvalidOperationException($"Where Condition is null and Model of [{TableInfo.Type}] do not has a PrimaryKey");
        }
        return sql.ToString();
    }
}