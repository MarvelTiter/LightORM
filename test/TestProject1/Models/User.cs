#nullable disable
namespace TestProject1.Models;



[LightTable(Name = "USER")]
public class User
{
    [LightColumn(Name = "USER_ID", PrimaryKey = true)]
    public string UserId { get; set; }
    [LightColumn(Name = "USER_NAME")]
    public string UserName { get; set; }
    [LightColumn(Name = "PASSWORD")]
    public string Password { get; set; }
    [LightColumn(Name = "AGE")]
    public int? Age { get; set; }
    [LightColumn(Name = "SIGN")]
    public string Sign { get; set; }
    [LightColumn(Name = "LAST_LOGIN")]
    public DateTime? LastLogin { get; set; }

    [LightColumn(Name = "IS_LOCK")]
    public bool IsLock { get; set; }

    [LightNavigate(ManyToMany = typeof(UserRole), MainName = nameof(UserId), SubName = nameof(UserRole.UserId))]
    public IEnumerable<Role> UserRoles { get; set; }

}



public class User2
{
    [LightColumn(Name = "USER_ID", PrimaryKey = true)]
    public string UserId { get; }
    [LightColumn(Name = "USER_NAME")]
    public string UserName { get; init; }
}
[LightTable(Name = "USER")]
public class UserFlat
{
    [LightColumn(Name = "USER_ID", PrimaryKey = true)]
    public string UserId { get; set; }
    [LightColumn(Name = "USER_NAME")]
    public string UserName { get; set; }
    [LightColumn(Name = "PASSWORD")]
    public string Password { get; set; }
    
    [LightColumn(Name = "SIGN")]
    public string Sign { get; set; }
    [LightColumn(Name = "LAST_LOGIN")]
    public DateTime LastLogin { get; set; }

    [LightNavigate(ManyToMany = typeof(UserRole), MainName = nameof(UserId), SubName = nameof(UserRole.UserId))]
    public IEnumerable<Role> UserRoles { get; set; }

    [LightFlat]
    public PrivateInfo? PriInfo { get; set; }
}

public class PrivateInfo
{
    [LightColumn(Name = "AGE")]
    public int? Age { get; set; }
    [LightColumn(Name = "IS_LOCK")]
    public bool IsLock { get; set; }
}
