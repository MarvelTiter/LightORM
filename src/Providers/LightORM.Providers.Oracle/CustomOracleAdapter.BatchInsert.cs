using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using LightORM.Utils;
using System.Text;

namespace LightORM.Providers.Oracle;

internal partial class CustomOracleAdapter
{
    public override void HandleBatchInsert(BatchActionContext context)
    {
        var batchs = context.Batchs;
        var builder = context.Builder;
        var insertColumns = context.TargetColumns;
        var database = context.ScopedAdapter;
        foreach (var item in batchs)
        {
            //StringBuilder sb = new("INSERT ALL");
            using var _ = StringBuilderPool.Get(out var sb);
            sb.AppendLine("INSERT ALL");
            for (int i = 0; i < item.Parameters.Count; i++)
            {
                AttachInserts(sb);
                List<SimpleColumn>? dic = item.Parameters[i];
                sb.Append('(');
                foreach (var c in dic)
                {
                    sb.Append(this.GetValueExpression(c));
                    sb.Append(',');
                }
                sb.RemoveLast(1);
                sb.AppendLine(")");
            }
            sb.Append("SELECT 1 FROM DUAL");
            builder.HandleSqlParameters(sb, this);
            item.Sql = sb.ToString();
        }


        void AttachInserts(StringBuilder sb)
        {
            sb.Append("    INTO ");
            sb.AppendTableName(database, builder.MainTable, false);
            sb.Append(" (");
            foreach (var item in insertColumns)
            {
                sb.AppendEmphasis(item.ColumnName, this);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.Append(')');
            sb.Append(" VALUES ");
        }
    }
}
