using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using LightORM.Utils;
using System.Text;

namespace LightORM.Providers.MySql;

internal partial class CustomMySqlAdapter
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
        var database = context.ScopedAdapter;
        var builder = context.Builder;
        var batchs = context.Batchs;
        var columns = context.TargetColumns;
        foreach (var batch in batchs)
        {
            using var _ = StringBuilderPool.Get(out var sb);
            builder.WriteTags(sb);
            if (builder.IgnoreOnConflict)
            {
                PreHandleInsertBuilder(sb);

                for (int i = 0; i < batch.RowParameters.Count; i++)
                {
                    List<SimpleColumn>? dic = batch.RowParameters[i];
                    if (i > 0)
                    {
                        sb.Append(',');
                        sb.AppendLine();
                    }
                    sb.Append('(');
                    foreach (var c in dic)
                    {
                        if (c.IsNewVersion)
                            continue;
                        //sb.Append(database.GetValueExpression(c));
                        sb.AppendSimpleColumnValueExpression(c, database);
                        sb.Append(',');
                    }
                    sb.RemoveLast(1);
                    sb.Append(')');
                }
                builder.HandleSqlParameters(sb, database);
                batch.Sql = sb.ToString();

                void PreHandleInsertBuilder(StringBuilder sb)
                {
                    sb.Append("INSERT IGNORE INTO ");
                    //sb.Append(GetTableName(database, MainTable, false));
                    sb.AppendTableName(database, builder.MainTable, false).AppendLine();
                    sb.Append('(');
                    foreach (var item in columns)
                    {
                        sb.AppendEmphasis(item.ColumnName, database);
                        sb.Append(',');
                    }
                    sb.RemoveLast(1);
                    sb.Append(')');
                    sb.AppendLine();
                    sb.AppendLine("VALUES");
                }
            }
            else
            {
                sb.Append("INSERT INTO ");
                //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
                sb.AppendTableName(database, context.Builder.MainTable, false).AppendLine();
                sb.Append('(');
                foreach (var item in columns)
                {
                    sb.AppendEmphasis(item.ColumnName, database);
                    sb.Append(',');
                }
                sb.RemoveLast(1);
                sb.AppendLine(")");
                BuildSource(batch);
                sb.AppendLine("ON DUPLICATE KEY UPDATE");
                var vc = columns.FirstOrDefault(c => c.IsVersionColumn);
                foreach (var item in columns)
                {
                    if (item.IsVersionColumn || item.IsPrimaryKey)
                        continue;
                    if (vc is not null)
                    {
                        sb.Append("    ").AppendEmphasis(item.ColumnName, database).Append(" = ")
                            .Append("IF(").AppendTableName(database, context.Builder.MainTable, false).Append('.').AppendEmphasis(vc.ColumnName, database)
                            .Append(" = ")
                            .Append("s.").AppendEmphasis(vc.ColumnName, database).Append(", ")
                            .Append("s.").AppendEmphasis(item.ColumnName, database).Append(", ")
                            .AppendTableName(database, context.Builder.MainTable, false).Append('.').AppendEmphasis(item.ColumnName, database).Append(')');
                    }
                    else
                    {
                        sb.Append("    ").AppendTableName(database, context.Builder.MainTable, false).Append('.').AppendEmphasis(item.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(item.ColumnName, database);
                    }
                    sb.AppendLine(",");
                }

                if (vc is not null)
                {
                    sb.Append("    ").AppendEmphasis(vc.ColumnName, database).Append(" = ")
                        .Append("IF(").AppendTableName(database, context.Builder.MainTable, false).Append('.').AppendEmphasis(vc.ColumnName, database)
                        .Append(" = ")
                        .Append("s.").AppendEmphasis(vc.ColumnName, database).Append(", ")
                        .Append("s.").Append(NEW_VERSION_COLUMN).Append(',')
                        .AppendTableName(database, context.Builder.MainTable, false).Append('.').AppendEmphasis(vc.ColumnName, database).Append(')').AppendLine(",");
                }
                sb.RemoveLast(SqlBuilder.N.Length + 1);


                void BuildSource(BatchSqlInfo batch)
                {
                    sb.Append("SELECT ");
                    foreach (var item in columns)
                    {
                        sb.AppendEmphasis(item.ColumnName, database);
                        sb.Append(',');
                    }
                    sb.RemoveLast(1);
                    sb.AppendLine();
                    sb.AppendLine("FROM (");
                    for (int i = 0; i < batch.RowParameters.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.AppendLine("    UNION ALL");
                        }
                        sb.Append("    SELECT ");
                        var rowData = batch.RowParameters[i];
                        foreach (var item in rowData)
                        {
                            if (item.IsNewVersion)
                            {
                                sb.WithPrefix(item.ValueName, database).Append(" AS ").Append(NEW_VERSION_COLUMN);
                            }
                            else
                            {
                                sb.WithPrefix(item.ValueName, database).Append(" AS ").AppendEmphasis(item.ColumnName, database);
                            }
                            sb.Append(',');
                        }
                        sb.RemoveLast(1);
                        sb.AppendLine();
                    }
                    sb.AppendLine(") AS s");
                }
            }
            batch.Sql = sb.ToString();
        }
    }
}
