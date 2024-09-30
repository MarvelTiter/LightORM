namespace LightORM.Extension
{
    internal static class BatchSqlInfoExtensions
    {
        public static Dictionary<string, object> ToDictionaryParameters(this BatchSqlInfo info)
        {
            Dictionary<string, object> values = [];
            foreach (var row in info.Parameters)
            {
                foreach (var col in row)
                {
                    if (col.Value == null) continue;
                    values.Add(col.ParameterName, col.Value);
                }
            }
            return values;
        }
    }
}
