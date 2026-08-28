using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;

namespace LightORM.Providers.SqlServer;

internal partial class CustomSqlServerAdapter
{
    public override void HandleInsertOrUpdate(UpsertContext context)
    {
        var database = context.ScopedAdapter;
        var columnValueMaps = context.ColumnValueMap;
        var sb = context.Sql;
        sb.Append("MERGE INTO ").AppendTableName(database, context.Builder.MainTable, false).AppendLine(" t");
        sb.Append("USING (SELECT ");
        foreach (var kv in columnValueMaps)
        {
            var e = kv.Value;
            var c = kv.Key;
            sb.Append(e.Value).Append(" AS ").Append(e.Column).Append(',');
            if (c.IsVersionColumn)
            {
                var oldVersion = context.Parameters[c.PropertyName];
                var newVersion = SqlBuilder.VersionPlus(oldVersion);
                var verionName = $"{c.PropertyName}_n";
                context.Parameters[verionName] = newVersion;
                sb.Append(' ');
                sb.WithPrefix(verionName, database).Append(" AS ").Append(verionName).Append(',');
            }
        }
        sb.RemoveLast(1);
        sb.AppendLine(") s");
        var whereKey = columnValueMaps.Keys.Where(c => c.IsPrimaryKey).ToArray();
        for (int i = 0; i < whereKey.Length; i++)
        {
            var k = whereKey[i];
            if (i == 0)
                sb.Append("ON (t.").AppendEmphasis(k.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, database);
            else
                sb.Append(" AND t.").AppendEmphasis(k.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, database);
        }
        sb.AppendLine(")");
        if (!context.IgnoreWhenMap)
        {
            sb.Append("WHEN MATCHED ");
            foreach (var item in columnValueMaps)
            {
                if (!item.Key.IsVersionColumn)
                    continue;
                sb.Append("AND (t.").AppendEmphasis(item.Key.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(item.Key.ColumnName, database);
                sb.Append(')');
                break;
            }
            sb.AppendLine(" THEN");
            sb.Append("    UPDATE SET");
            foreach (var kv in columnValueMaps)
            {
                if (kv.Key.IsPrimaryKey)
                    continue;
                if (kv.Key.IsVersionColumn)
                {
                    var verionName = $"{kv.Key.PropertyName}_n";
                    sb.Append(' ');
                    sb.Append(kv.Value.Column).Append(" = ").Append("s.").Append(verionName).Append(',');
                }
                else
                {
                    sb.Append(' ');
                    sb.Append(kv.Value.Column).Append(" = ").Append("s.").Append(kv.Value.Column).Append(',');
                }
            }
            sb.RemoveLast(1);
            sb.AppendLine();
            //foreach (var kv in columnValueMaps)
            //{
            //    if (!kv.Key.IsVersionColumn)
            //        continue;
            //    sb.Append("    WHERE ");
            //    sb.Append(kv.Value.Column).Append(" = ").Append(kv.Value.Value).AppendLine();
            //}
        }
        sb.AppendLine("WHEN NOT MATCHED THEN");
        sb.Append("    INSERT (")
            .AppendEntryColumns(columnValueMaps.Values)
            .Append(") VALUES (")
            .AppendEntryColumns(columnValueMaps.Values, "s.")
            .Append(");");
    }
}
