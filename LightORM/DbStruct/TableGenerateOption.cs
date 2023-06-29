namespace MDbContext.DbStruct
{
    public class TableGenerateOption
    {
        public bool NotCreateIfExists { get; set; }
        public bool UseUnicodeString { get; set; } = true;
        public bool SupportComment { get; set; }

        private int defaultStringLength = 256;
        public int DefaultStringLength
        {
            get => UseUnicodeString ? defaultStringLength / 2 : defaultStringLength;
            set => defaultStringLength = value;
        }
    }
}
