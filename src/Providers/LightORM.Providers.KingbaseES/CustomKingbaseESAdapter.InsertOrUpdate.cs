using LightORM.Builder;
using LightORM.Extension;
using LightORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.KingbaseES;

internal partial class CustomKingbaseESAdapter
{
    public override StringBuilder HandleInsertOrUpdate(UpsertContext context)
    {
        var database = context.ScopedAdapter;
        var columnValueMaps = context.ColumnValueMap;
        var sb = new StringBuilder();

        OnConflict();

        return sb;

        void OnConflict()
        {
            sb = new("INSERT INTO ");
            //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
            sb.AppendTableName(database, context.Builder.MainTable, false).AppendLine();
            sb.Append('(');
            sb.AppendEntryColumns(columnValueMaps.Values);
            sb.AppendLine(")");
            sb.AppendLine("VALUES");
            sb.Append('(');
            sb.AppendEntryValues(columnValueMaps.Values);
            sb.AppendLine(")");
            var whereKey = columnValueMaps.Where(c => c.Key.IsPrimaryKey).ToArray();
            sb.Append("ON CONFLICT (");
            for (int i = 0; i < whereKey.Length; i++)
            {
                var k = whereKey[i];
                sb.Append(k.Value.Column);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.AppendLine(")");
            if (context.IgnoreWhenMap)
            {
                sb.Append("DO NOTHING");
            }
            else
            {
                sb.Append("DO UPDATE SET");
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
                        sb.Append(kv.Value.Column).Append(" = ").WithPrefix(verionName, database).Append(',');
                    }
                    else
                    {
                        sb.Append(' ');
                        // EXCLUDED 是 PostgreSQL 在 INSERT ... ON CONFLICT 语句中的一个特殊关键字，代表尝试插入的那行新数据。
                        sb.Append(kv.Value.Column).Append(" = ").Append("EXCLUDED.").Append(kv.Value.Column).Append(',');
                    }
                }
                sb.RemoveLast(1);
                sb.AppendLine();
                foreach (var kv in columnValueMaps)
                {
                    if (!kv.Key.IsVersionColumn)
                        continue;
                    sb.Append("    WHERE ");
                    sb.AppendTableName(database, context.Builder.MainTable, false)
                        .Append('.')
                        .Append(kv.Value.Column).Append(" = ").Append(kv.Value.Value).Append(' ');
                }
            }
        }


        //void Merge()
        //{
        //    sb.Append("MERGE INTO ").AppendTableName(database, context.Builder.MainTable, false).AppendLine(" t");
        //    sb.Append("USING (SELECT ");
        //    foreach (var e in columnValueMaps.Values)
        //    {
        //        sb.Append(e.Value).Append(" AS ").Append(e.Column).Append(',');
        //    }
        //    sb.RemoveLast(1);
        //    sb.AppendLine(" ) s");
        //    var whereKey = columnValueMaps.Keys.Where(c => c.IsPrimaryKey).ToArray();
        //    for (int i = 0; i < whereKey.Length; i++)
        //    {
        //        var k = whereKey[i];
        //        if (i == 0)
        //            sb.Append("ON (t.").AppendEmphasis(k.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, database);
        //        else
        //            sb.Append(" AND t.").AppendEmphasis(k.ColumnName, database).Append(" = ").Append("s.").AppendEmphasis(k.ColumnName, database);
        //    }
        //    sb.AppendLine(")");
        //    if (!context.IgnoreWhenMap)
        //    {
        //        sb.AppendLine("WHEN MATCHED THEN");
        //        sb.Append("    UPDATE SET");
        //        foreach (var kv in columnValueMaps)
        //        {
        //            if (kv.Key.IsPrimaryKey)
        //                continue;
        //            if (kv.Key.IsVersionColumn)
        //            {
        //                var oldVersion = context.Parameters[kv.Key.PropertyName];
        //                var newVersion = SqlBuilder.VersionPlus(oldVersion);
        //                var verionName = $"{kv.Key.PropertyName}_n";
        //                context.Parameters[verionName] = newVersion;
        //                sb.Append(' ');
        //                sb.Append(kv.Value.Column).Append(" = ").WithPrefix(verionName, database).Append(',');
        //            }
        //            else
        //            {
        //                sb.Append(' ');
        //                sb.Append(kv.Value.Column).Append(" = ").Append("s.").Append(kv.Value.Column).Append(',');
        //            }
        //        }
        //        sb.RemoveLast(1);
        //        sb.AppendLine();
        //        foreach (var kv in columnValueMaps)
        //        {
        //            if (!kv.Key.IsVersionColumn)
        //                continue;
        //            sb.Append("    WHERE ");
        //            sb.Append(kv.Value.Column).Append(" = ").Append(kv.Value.Value).Append(' ');
        //        }
        //    }
        //    sb.AppendLine("WHEN NOT MATCHED THEN");
        //    sb.Append("    INSERT (")
        //        .AppendEntryColumns(columnValueMaps.Values)
        //        .Append(") VALUES (")
        //        .AppendEntryValues(columnValueMaps.Values)
        //        .Append(')');
        //}
    }

}
