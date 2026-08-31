using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;

namespace LightORM.Providers.MySql;

internal partial class CustomMySqlAdapter
{
    public override void HandleInsertOrUpdate(UpsertContext context)
    {
        var database = context.ScopedAdapter;
        var columnValueMaps = context.ColumnValueMap;
        var sb = context.Sql;
        if (context.IgnoreWhenMap)
        {
            sb.Append("INSERT IGNORE INTO ");
            sb.AppendTableName(database, context.Builder.MainTable, false).AppendLine();
            sb.Append('(');
            sb.AppendEntryColumns(columnValueMaps.Values);
            sb.AppendLine(")");
            BuildSource();
        }
        else
        {
            sb.Append("INSERT INTO ");
            //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
            sb.AppendTableName(database, context.Builder.MainTable, false).AppendLine();
            sb.Append('(');
            sb.AppendEntryColumns(columnValueMaps.Values);
            sb.AppendLine(")");
            BuildSource();
            sb.AppendLine("ON DUPLICATE KEY UPDATE");
            var vc = columnValueMaps.FirstOrDefault(c => c.Key.IsVersionColumn);
            foreach (var item in columnValueMaps)
            {
                if (item.Key.IsVersionColumn || item.Key.IsPrimaryKey)
                    continue;
                if (vc.Key is not null)
                {
                    sb.Append("    ").Append(item.Value.Column).Append(" = ")
                        .Append("IF(").AppendTableName(database, context.Builder.MainTable, false).Append('.').Append(vc.Value.Column)
                        .Append(" = ")
                        .Append("s.").Append(vc.Value.Column).Append(", ")
                        .Append("s.").Append(item.Value.Column).Append(", ")
                        .AppendTableName(database, context.Builder.MainTable, false).Append('.').Append(item.Value.Column).Append(')');
                }
                else
                {
                    sb.Append("    ").AppendTableName(database, context.Builder.MainTable, false).Append('.').Append(item.Value.Column).Append(" = ").Append("s.").Append(item.Value.Column);
                }
                sb.AppendLine(",");
            }

            if (vc.Key is not null)
            {
                var verionName = $"{vc.Key.PropertyName}_n";
                sb.Append("    ").Append(vc.Value.Column).Append(" = ")
                    .Append("IF(").AppendTableName(database, context.Builder.MainTable, false).Append('.').Append(vc.Value.Column)
                    .Append(" = ")
                    .Append("s.").Append(vc.Value.Column).Append(", ")
                    .Append("s.").Append(verionName).Append(',')
                    .AppendTableName(database, context.Builder.MainTable, false).Append('.').Append(vc.Value.Column).Append(')').AppendLine(",");
            }
            sb.RemoveLast(SqlBuilder.N.Length + 1);
        }


        void BuildSource()
        {
            sb.Append("SELECT ");
            foreach (var item in columnValueMaps)
            {
                sb.Append(item.Value.Column);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.AppendLine();
            sb.AppendLine("FROM (");
            sb.Append("    SELECT ");
            foreach (var item in columnValueMaps)
            {
                sb.Append(item.Value.Value).Append(" AS ").Append(item.Value.Column);
                sb.Append(',');
                if (item.Key.IsVersionColumn)
                {
                    var oldVersion = context.Parameters[item.Key.PropertyName];
                    var newVersion = SqlBuilder.VersionPlus(oldVersion);
                    var verionName = $"{item.Key.PropertyName}_n";
                    context.Parameters[verionName] = newVersion;
                    sb.WithPrefix(verionName, database).Append(" AS ").Append(verionName);
                    sb.Append(',');
                }
            }
            sb.RemoveLast(1);
            sb.AppendLine();
            sb.AppendLine(") AS s");
        }
    }


}
