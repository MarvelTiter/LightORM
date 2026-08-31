namespace LightORM.Extension
{
    internal static class BatchSqlInfoExtensions
    {
        public static Dictionary<string, object> ToDictionaryParameters(this BatchSqlInfo info)
        {
            Dictionary<string, object> values = [];
            foreach (var row in info.RowParameters)
            {
                foreach (var col in row)
                {
                    if (col.Value == null) continue;
                    if (col.IsStaticValue) continue;
                    values.Add(col.ValueName, col.Value);
                }
            }
            return values;
        }
    }
}
