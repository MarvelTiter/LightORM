using LightORM.DbStruct;

namespace DatabaseUtils.Models
{
    public class DatabaseTable(ReadedTable table)
    {
        public ReadedTable Table { get; set; } = table;
        public string? GeneratedResult { get; set; }
        public string? CsFileName { get; set; }
        public bool IsSelected { get; set; }
        public Config? Config { get; set; }
    }
}