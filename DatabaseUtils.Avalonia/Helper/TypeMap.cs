namespace DatabaseUtils.Helper
{
    public class TypeMap
    {
        public static bool Map(string dbType, string nullable, out string type)
        {
            var d = dbType.ToLower();
            if (d.Contains("char"))
            {
                type = nullable == "YES" ? "string?" : "string";
            }
            else if (d.Contains("int")
                || d.Contains("bit")
                || d.Contains("number")
                || d.Contains("decimal")
                )
            {
                type = nullable == "YES" ? "int?" : "int";
            }
            else if (d.Contains("date"))
            {
                type = nullable == "YES" ? "DateTime?" : "DateTime";
            }
            else
            {
                type = "";
                return false;
            }
            return true;
        }
        public TypeMap()
        {

        }
    }
}
