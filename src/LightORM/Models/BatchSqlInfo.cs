namespace LightORM.Models
{
    internal readonly record struct SimpleColumn(ITableColumnInfo Column
        , string ValueName
        , int RowIndex
        , bool IsNewVersion
        , object? Value
        , bool IsStaticValue)
    {
        public bool IsPrimaryKey => Column.IsPrimaryKey;
        public bool IsVersion => Column.IsVersionColumn;
        public string PropName => Column.PropertyName;
        public string ColumnName => Column.ColumnName;

    }
    //internal record BatchParameters(ITableColumnInfo Column, List<SimpleColumn> Parameters);
    internal class BatchSqlInfo(List<List<SimpleColumn>> parameters, int index)
    {
        public int Index { get; set; } = index;
        public string? Sql { get; set; }
        public List<List<SimpleColumn>> RowParameters { get; set; } = parameters;
    }
}
