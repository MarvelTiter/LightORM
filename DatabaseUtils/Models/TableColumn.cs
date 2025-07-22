using System.Diagnostics.CodeAnalysis;

namespace DatabaseUtils.Models
{
    public class TableColumn
    {
        [NotNull] public string? ColumnName { get; set; }
        [NotNull] public string? DataType { get; set; }
        [NotNull] public string? IsPrimaryKey { get; set; }
        [NotNull] public string? IsIdentity { get; set; }
        [NotNull] public string? Nullable { get; set; }
        [NotNull] public string? Comments { get; set; }
        [NotNull] public string? DefaultValue { get; set; }
        [NotNull] public string? Length { get; set; }

    }
}
