using System;

namespace MDbContext.DbStruct
{
    internal struct DbColumn
    {
        public string Name { get; set; }
        public bool PrimaryKey { get; set; }
        public bool AutoIncrement { get; set; }
        public bool NotNull { get; set; }
        public int? Length { get; set; }
        public object? Default { get; set; }
        public string? Comment { get; set; }
        public Type DataType { get; set; }
    }
}
