using LightORM.Extension;
using LightORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Oracle;

internal partial class CustomOracleAdapter
{
    public override void HandleBatchInsert(BatchActionContext context)
    {
        var batchs = context.Batchs;
        var builder = context.Builder;
        var insertColumns = context.InsertColumns;
        foreach (var item in batchs)
        {
            StringBuilder sb = new("INSERT ALL");
            sb.AppendLine();
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
            sb.AppendTableName(this, builder.MainTable, false);
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
