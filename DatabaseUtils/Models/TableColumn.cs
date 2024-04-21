using System.Diagnostics.CodeAnalysis;

namespace DatabaseUtils.Models
{
    public class TableColumn
    {
        [NotNull] public string? ColumnName { get; set; }
        [NotNull] public string? DataType { get; set; }
        [NotNull] public string? Nullable { get; set; }
        [NotNull] public string? Comments { get; set; }
    }
}
