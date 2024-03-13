namespace LightORM.Utils;

public static class TypeExtensions
{
    public static string GetColumnNameFromType(this Type self, string propertyName)
    {
        var prop = self.GetProperty(propertyName);
        var attr = Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute));
        if (attr is ColumnAttribute col)
        {
            return col.Name;
        }
        return prop.Name;
    }
}
