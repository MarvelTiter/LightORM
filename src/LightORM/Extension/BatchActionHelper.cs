namespace LightORM.Extension
{
    internal static class BatchActionHelper
    {
        /// <summary>
        /// 计算批次数量
        /// </summary>
        /// <param name="column">列数，对应参数个数</param>
        /// <param name="limit">参数个数最大值</param>
        /// <param name="dataCount">更新实体数量</param>
        /// <param name="rows">每次更新的行数</param>
        /// <returns></returns>
        private static int CalcBatchSize(int column, int limit, int dataCount, out int rows)
        {
            rows = limit / column;
            if (rows == 0) rows = 1;
            var size = dataCount / rows;
            if (size == 0) size = 1;

            if (size * rows < dataCount)
            {
                size++;
            }
            return size;
        }
        public static List<BatchSqlInfo> GenBatchInfos<T>(this ITableColumnInfo[] columns
            , T[] datas
            , int limit = 2000
            , Dictionary<string, object>? additionalParameters = null)
        {
            var list = new List<BatchSqlInfo>();
            var verions = columns.Count(c => c.IsVersionColumn);
            var size = CalcBatchSize(columns.Length + verions, limit, datas.Length, out var rows);
            for (var i = 0; i < size; i++)
            {
                var rowIndex = 0;
                var batchList = datas.Skip(i * rows).Take(rows);
                var pList = new List<List<SimpleColumn>>();
                foreach (var obj in batchList)
                {
                    var dbParameters = new List<SimpleColumn>();
                    foreach (var col in columns)
                    {
                        var isStatic = GetValue(col, obj!, additionalParameters, out var val);
                        dbParameters.Add(new SimpleColumn(col.IsPrimaryKey, col.IsVersionColumn, col.ColumnName, $"{col.PropertyName}_{rowIndex}", col.PropertyName, val, isStatic)
                        );
                    }
                    rowIndex++;
                    pList.Add(dbParameters);
                }

                list.Add(new BatchSqlInfo(pList, i + 1));
            }
            return list;

            static bool GetValue(ITableColumnInfo col, object target, Dictionary<string, object>? additionalParameters, out object? value)
            {
                if (additionalParameters != null && additionalParameters.TryGetValue(col.PropertyName, out value))
                {
                    return true;
                }
                value = col.GetValue(target);
                return false;
            }
        }
    }
}
