using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            sb.AppendLine("VALUES");
            sb.Append('(');
            sb.AppendEntryValues(columnValueMaps.Values);
            sb.AppendLine(")");
        }
        else
        {
            sb.Append("INSERT INTO ");
            //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
            sb.AppendTableName(database, context.Builder.MainTable, false).AppendLine();
            sb.Append('(');
            sb.AppendEntryColumns(columnValueMaps.Values);
            sb.AppendLine(")");
            sb.AppendLine("VALUES");
            sb.Append('(');
            sb.AppendEntryValues(columnValueMaps.Values);
            sb.AppendLine(")");
            sb.AppendLine("ON DUPLICATE KEY UPDATE");
            var vc = columnValueMaps.FirstOrDefault(c => c.Key.IsVersionColumn);
            foreach (var item in columnValueMaps)
            {
                if (item.Key.IsVersionColumn || item.Key.IsPrimaryKey)
                    continue;
                if (vc.Key is not null)
                {
                    sb.Append("    ").Append(item.Value.Column).Append(" = ").Append("IF(").Append(vc.Value.Column).Append(" = ").Append(vc.Value.Value)
                        .Append(", ").Append("VALUES(").Append(item.Value.Column).Append("), ").Append(item.Value.Column).Append(')');
                }
                else
                {
                    sb.Append("    ").Append(item.Value.Column).Append(" = ").Append("VALUES(").Append(item.Value.Column).Append(')');
                }
                sb.AppendLine(",");
            }

            if (vc.Key is not null)
            {
                var oldVersion = context.Parameters[vc.Key.PropertyName];
                var newVersion = SqlBuilder.VersionPlus(oldVersion);
                var verionName = $"{vc.Key.PropertyName}_n";
                context.Parameters[verionName] = newVersion;
                sb.Append("    ").Append(vc.Value.Column).Append(" = ").Append("IF(").Append(vc.Value.Column).Append(" = ").WithPrefix(vc.Key.PropertyName, database).Append(", ").WithPrefix(verionName, database).Append(',').Append(vc.Value.Column).Append(')').AppendLine(",");
            }
            sb.RemoveLast(SqlBuilder.N.Length + 1);
        }
        
    }
}
