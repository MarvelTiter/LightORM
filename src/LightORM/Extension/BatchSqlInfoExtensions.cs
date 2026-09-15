namespace LightORM.Extension
{
    internal static class BatchSqlInfoExtensions
    {
        public static Dictionary<string, DbParameterValue> ToDictionaryParameters(this BatchSqlInfo info)
        {
            Dictionary<string, DbParameterValue> values = [];
            foreach (var row in info.RowParameters)
            {
                foreach (var col in row)
                {
                    if (col.Value == null) continue;
                    // 静态值(batch 下由 additionalParameters 提供的同一值)通常直接内联进 SQL 而不下发参数,
                    // 但 json 列无法作为字面量渲染(见 AppendSimpleColumnValueExpression), 仍需以参数下发。
                    if (col.IsStaticValue && !col.IsJsonColumn) continue;
                    // 参数值保持原始 CLR 值(含 json 列), 序列化/驱动类型化交由方言的 IDatabaseParameterBinder。
                    values.Add(col.ValueName, new(col.Column, col.Value));
                }
            }
            return values;
        }
    }
}
