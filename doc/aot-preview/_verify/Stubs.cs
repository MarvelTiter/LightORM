// 仅为验证生成代码可编译而存在的存根，非项目代码
global using LightORM;
global using LightORM.DbEntity.Attributes;

namespace LightORM;

public class LightTableAttribute : Attribute
{
    public string? Name { get; set; }
    public string? DatabaseKey { get; set; }
    public string? Schema { get; set; }
}

namespace LightORM.DbEntity.Attributes;

public class LightColumnAttribute : Attribute
{
    public string? Name { get; set; }
    public bool PrimaryKey { get; set; }
    public bool AutoIncrement { get; set; }
    public string? Comment { get; set; }
    public bool Ignore { get; set; }
    public bool NotNull { get; set; }
    public int Length { get; set; }
    public object? Default { get; set; }
    public bool Version { get; set; }
    public bool IgnoreUpdate { get; set; }
    public bool IgnoreInsert { get; set; }
}

public class LightNavigateAttribute : Attribute
{
    public LightNavigateAttribute() { }
    public LightNavigateAttribute(string mainName, string subName)
    {
        MainName = mainName;
        SubName = subName;
    }

    public LightNavigateAttribute(Type manyToMany, string mainName, string subName)
    {
        ManyToMany = manyToMany;
        MainName = mainName;
        SubName = subName;
    }

    public Type? ManyToMany { get; set; }
    public string? MainName { get; set; }
    public string? SubName { get; set; }
}

public class LightFlatAttribute : Attribute { }

public class LightJsonMapAttribute : Attribute { }

namespace LightORM.Utils;

public static class JsonHandler
{
    public static T? Deserialize<T>(string text) => default;
}

namespace LightORMTest.Models;

public class Role
{
    public string RoleId { get; set; } = string.Empty;
}

public class UserRole
{
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
}
