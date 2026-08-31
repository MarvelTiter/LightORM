using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using LightORM.Utils;

namespace LightORM.Providers.Dameng;

internal partial class CustomDamengAdapter
{
    public override void HandleBatchInsert<T>(BatchActionContext<InsertBuilder<T>> context)
    {
        if (context.Builder.UpdateOnConflict || context.Builder.IgnoreOnConflict)
        {
            UpsertBatchInsert(context);
        }
        else
        {
            base.HandleBatchInsert(context);
        }
    }

    private static void UpsertBatchInsert<T>(BatchActionContext<InsertBuilder<T>> context)
    {
        var builder = context.Builder;
        var database = context.ScopedAdapter;
        var batchs = context.Batchs;
        var columns = context.TargetColumns;
        foreach (var batch in batchs)
        {
            using var _ = StringBuilderPool.Get(out var sb);
            builder.WriteTags(sb);
            sb.Append("MERGE INTO ").AppendTableName(database, context.Builder.MainTable, false).AppendLine(" t");
            sb.AppendLine("USING (");
            for (int i = 0; i < batch.RowParameters.Count; i++)
            {
                List<SimpleColumn>? row = batch.RowParameters[i];
                if (i > 0)
                {
                    sb.AppendLine("    UNION ALL");
                }
                sb.Append("    SELECT ");
                foreach (var c in row)
                {
                    if (c.IsStaticValue)
                    {
                        //sb.Append(database.GetValueExpression(c));
                        sb.AppendSimpleColumnValueExpression(c, database);
                    }
                    else
                    {
                        //sb.WithPrefix(c.ParameterName, database);
                        sb.AppendSimpleColumnParameter(c, database);
                    }
                    if (c.IsNewVersion)
                    {
                        sb.Append(" AS ").Append(c.PropName).Append("_n").Append(',');
                    }
                    else
                    {
                        sb.Append(" AS ").AppendEmphasis(c.ColumnName, database).Append(',');
                    }
                }
                sb.RemoveLast(1);
                sb.AppendLine(" FROM DUAL");
            }
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
                sb.AppendLine("WHEN MATCHED THEN");
                sb.Append("    UPDATE SET");
                foreach (var kv in columns)
                {
                    if (kv.IsPrimaryKey)
                        continue;
                    if (kv.IsVersionColumn)
                    {
                        var verionName = $"{kv.PropertyName}_n";
                        sb.Append(' ');
                        sb.AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("s.").Append(verionName).Append(',');
                    }
                    else
                    {
                        sb.Append(' ');
                        sb.AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(kv.ColumnName, database).Append(',');
                    }
                }
                sb.RemoveLast(1);
                sb.AppendLine();
                foreach (var kv in columns)
                {
                    if (!kv.IsVersionColumn)
                        continue;
                    sb.Append("    WHERE ");
                    sb.Append("t.").AppendEmphasis(kv.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(kv.ColumnName, database).Append(' ');
                }
                sb.AppendLine();
            }
            sb.AppendLine("WHEN NOT MATCHED THEN");
            sb.Append("    INSERT (")
                .AppendEntryColumns(columns, database)
                .Append(") VALUES (")
                .AppendEntryColumns(columns, database, "s.")
                .Append(')');
            batch.Sql = sb.ToString();
        }
    }
}
