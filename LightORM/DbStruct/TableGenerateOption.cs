namespace MDbContext.DbStruct
{
    public class TableGenerateOption
    {
        public bool NotCreateIfExists { get; set; }
        public bool UseUnicodeString { get; set; } = true;
        public int DefaultStringLength { get; set; } = 256;
    }
}
