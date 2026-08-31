using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using LightORM.Utils;

namespace LightORM.Providers.KingbaseES;

partial class CustomKingbaseESAdapter
{
    public override void HandleBatchInsert<T>(BatchActionContext<InsertBuilder<T>> context)
    {
        if (context.Builder.UpdateOnConflict || context.Builder.IgnoreOnConflict)
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
        var batchs = context.Batchs;
        var database = context.ScopedAdapter;
        var columns = context.TargetColumns;
        foreach (var batch in batchs)
        {
            using var _ = StringBuilderPool.Get(out var sb);
            builder.WriteTags(sb);
            sb.Append("INSERT INTO ");
            //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
            sb.AppendTableName(database, context.Builder.MainTable, false);
            sb.Append('(');
            foreach (var item in columns)
            {
                sb.AppendEmphasis(item.ColumnName, database);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.AppendLine(")");
            BuildSource(batch);
            var whereKey = columns.Where(c => c.IsPrimaryKey).ToArray();
            sb.Append("ON CONFLICT (");
            for (int i = 0; i < whereKey.Length; i++)
            {
                var k = whereKey[i];
                sb.AppendEmphasis(k.ColumnName, database);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.AppendLine(")");
            if (builder.IgnoreOnConflict)
            {
                sb.Append("DO NOTHING");
            }
            else
            {
                sb.AppendLine("DO UPDATE SET");
                bool f1 = true;
                foreach (var kv in columns)
                {
                    if (kv.IsPrimaryKey)
                        continue;
                    if (!f1)
                    {
                        sb.AppendLine(",");
                    }
                    if (kv.IsVersionColumn)
                    {
                        sb.Append("    ");
                        sb.AppendEmphasis(kv.ColumnName, database).Append(" = CASE");
                        foreach (var item in batch.RowParameters)
                        {
                            bool first = true;
                            SimpleColumn versionCol = default;
                            foreach (var cell in item)
                            {
                                if (cell.IsNewVersion)
                                    versionCol = cell;
                                if (!cell.IsPrimaryKey)
                                    continue;
                                if (first)
                                {
                                    sb.AppendLine();
                                    sb.Append("        WHEN EXCLUDED.");
                                }
                                else
                                    sb.Append(" AND EXCLUDED.");
                                sb.AppendEmphasis(cell.ColumnName, database).Append(" = ").WithPrefix(cell.ValueName, database);
                                first = false;
                            }
                            sb.Append(" THEN ").WithPrefix(versionCol.ValueName, database);
                        }
                        sb.AppendLine();
                        sb.Append("    END ");
                    }
                    else
                    {
                        sb.Append("    ");
                        // EXCLUDED 是 PostgreSQL 在 INSERT ... ON CONFLICT 语句中的一个特殊关键字，代表尝试插入的那行新数据。
                        sb.AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("EXCLUDED.").AppendEmphasis(kv.ColumnName, database);
                    }
                    f1 = false;
                }
                sb.AppendLine();
                foreach (var kv in columns)
                {
                    if (!kv.IsVersionColumn)
                        continue;
                    sb.Append("    WHERE ");
                    sb.AppendTableName(database, context.Builder.MainTable, false)
                        .Append('.')
                        .AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("EXCLUDED.").AppendEmphasis(kv.ColumnName, database).Append(' ');
                }
            }
            batch.Sql = sb.ToString();

            void BuildSource(BatchSqlInfo batch)
            {
                sb.AppendLine("VALUES");
                for (int i = 0; i < batch.RowParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("    (");
                    var rowData = batch.RowParameters[i];
                    foreach (var item in rowData)
                    {
                        if (item.IsNewVersion)
                        {
                            continue;
                        }
                        sb.WithPrefix(item.ValueName, database);
                        sb.Append(',');
                    }
                    sb.RemoveLast(1);
                    sb.Append(')');
                }
                sb.AppendLine();
            }
        }
    }
}
