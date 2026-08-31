using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using LightORM.Utils;

namespace LightORM.Providers.SqlServer;

partial class CustomSqlServerAdapter
{
    const string NEW_VERSION_COLUMN = "NewVersionValue";
    public override void HandleBatchInsert<T>(BatchActionContext<InsertBuilder<T>> context)
    {
        if (context.Builder.IgnoreOnConflict || context.Builder.UpdateOnConflict)
        {
            HandleBatchUpsert(context);
        }
        else
        {
            base.HandleBatchInsert(context);
        }
    }

    private static void HandleBatchUpsert<T>(BatchActionContext<InsertBuilder<T>> context)
    {
        var builder = context.Builder;
        var database = context.ScopedAdapter;
        var columns = context.TargetColumns;
        var batchs = context.Batchs;
        foreach (var batch in batchs)
        {
            using var _ = StringBuilderPool.Get(out var sb);
            builder.WriteTags(sb);
            sb.Append("MERGE INTO ").AppendTableName(database, context.Builder.MainTable, false).AppendLine(" t");
            sb.Append("USING (");
            for (int r = 0; r < batch.RowParameters.Count; r++)
            {
                List<SimpleColumn>? row = batch.RowParameters[r];
                sb.AppendLine();
                if (r > 0)
                {
                    sb.AppendLine("    UNION ALL");
                }
                sb.Append("    SELECT ");
                for (int i = 0; i < row.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }
                    SimpleColumn cell = row[i];
                    if (cell.IsNewVersion)
                    {
                        sb.WithPrefix(cell.ValueName, database).Append(" AS ").Append(NEW_VERSION_COLUMN);
                    }
                    else
                    {
                        sb.WithPrefix(cell.ValueName, database).Append(" AS ").AppendEmphasis(cell.ColumnName, database);
                    }
                }
            }
            sb.AppendLine();
            sb.AppendLine(") s");
            var whereKey = columns.Where(c => c.IsPrimaryKey).ToArray();
            for (int i = 0; i < whereKey.Length; i++)
            {
                var k = whereKey[i];
                if (i == 0)
                    sb.Append("ON (t.").AppendEmphasis(k.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, database);
                else
                    sb.Append(" AND t.").AppendEmphasis(k.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, database);
            }
            sb.AppendLine(")");
            if (!builder.IgnoreOnConflict)
            {
                sb.Append("WHEN MATCHED ");
                foreach (var item in columns)
                {
                    if (!item.IsVersionColumn)
                        continue;
                    sb.Append("AND (t.").AppendEmphasis(item.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(item.ColumnName, database);
                    sb.Append(')');
                    break;
                }
                sb.AppendLine(" THEN");
                sb.Append("    UPDATE SET");
                foreach (var kv in columns)
                {
                    if (kv.IsPrimaryKey)
                        continue;
                    if (kv.IsVersionColumn)
                    {
                        sb.Append(' ');
                        sb.AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("s.").Append(NEW_VERSION_COLUMN).Append(',');
                    }
                    else
                    {
                        sb.Append(' ');
                        sb.AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(kv.ColumnName, database).Append(',');
                    }
                }
                sb.RemoveLast(1);
                sb.AppendLine();
            }
            sb.AppendLine("WHEN NOT MATCHED THEN");
            sb.Append("    INSERT (")
                .AppendEntryColumns(columns, database)
                .Append(") VALUES (")
                .AppendEntryColumns(columns, database, "s.")
                .Append(");");
            batch.Sql = sb.ToString();
        }

    }
}
