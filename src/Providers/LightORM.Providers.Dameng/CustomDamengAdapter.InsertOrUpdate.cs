using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Dameng;

internal partial class CustomDamengAdapter
{
    public override StringBuilder HandleInsertOrUpdate(UpsertContext context)
    {
        var columnValueMaps = context.ColumnValueMap;
        var sb = new StringBuilder();
        sb.Append("MERGE INTO ").AppendTableName(this, context.Builder.MainTable, false).AppendLine(" t");
        sb.Append("USING (SELECT ");
        foreach (var e in columnValueMaps.Values)
        {
            sb.Append(e.Value).Append(" AS ").Append(e.Column).Append(',');
        }
        sb.RemoveLast(1);
        sb.AppendLine(" FROM DUAL ) s");
        var whereKey = columnValueMaps.Keys.Where(c => c.IsPrimaryKey).ToArray();
        for (int i = 0; i < whereKey.Length; i++)
        {
            var k = whereKey[i];
            if (i == 0)
                sb.Append("ON (t.").AppendEmphasis(k.ColumnName, this).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, this);
            else
                sb.Append(" AND t.").AppendEmphasis(k.ColumnName, this).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, this);
        }
        sb.AppendLine(")");
        if (!context.IgnoreWhenMap)
        {
            sb.AppendLine("WHEN MATCHED THEN");
            sb.Append("    UPDATE SET");
            foreach (var kv in columnValueMaps)
            {
                if (kv.Key.IsPrimaryKey)
                    continue;
                if (kv.Key.IsVersionColumn)
                {
                    var oldVersion = context.Parameters[kv.Key.PropertyName];
                    var newVersion = SqlBuilder.VersionPlus(oldVersion);
                    var verionName = $"{kv.Key.PropertyName}_n";
                    context.Parameters[verionName] = newVersion;
                    sb.Append(' ');
                    sb.Append(kv.Value.Column).Append(" = ").WithPrefix(verionName, this).Append(',');
                }
                else
                {
                    sb.Append(' ');
                    sb.Append(kv.Value.Column).Append(" = ").Append("s.").Append(kv.Value.Column).Append(',');
                }
            }
            sb.RemoveLast(1);
            sb.AppendLine();
            foreach (var kv in columnValueMaps)
            {
                if (!kv.Key.IsVersionColumn)
                    continue;
                sb.Append("    WHERE ");
                sb.Append(kv.Value.Column).Append(" = ").Append(kv.Value.Value).Append(' ');
            }
            sb.AppendLine();
        }
        sb.AppendLine("WHEN NOT MATCHED THEN");
        sb.Append("    INSERT (")
            .AppendEntryColumns(columnValueMaps.Values)
            .Append(") VALUES (")
            .AppendEntryValues(columnValueMaps.Values)
            .Append(')');
        return sb;
    }

}
