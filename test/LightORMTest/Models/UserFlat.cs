#nullable disable

namespace LightORMTest.Models;

[LightTable(Name = "USER_FLAT")]
public class UserFlat
{
    [LightColumn(Name = "USER_ID", PrimaryKey = true)]
    public string UserId { get; set; } = string.Empty;
    [LightColumn(Name = "USER_NAME")]
    public string UserName { get; set; } = string.Empty;
    [LightColumn(Name = "PASSWORD")]
    public string Password { get; set; } = string.Empty;

    [LightColumn(Name = "SIGN")]
    public string Sign { get; set; } = string.Empty;
    [LightColumn(Name = "LAST_LOGIN")]
    public DateTime LastLogin { get; set; }

    [LightNavigate(ManyToMany = typeof(UserRole), MainName = nameof(UserId), SubName = nameof(UserRole.UserId))]
    public IEnumerable<Role> UserRoles { get; set; } = [];

    [LightColumn(Name = "VER", Version = true)]
    public int Version { get; set; }

    [LightFlat]
    public PrivateInfo? PriInfo { get; set; }
}
public class PrivateInfo
{
    [LightColumn(Name = "AGE")]
    public int? Age { get; set; } = 10;
    [LightColumn(Name = "IS_LOCK")]
    public bool IsLock { get; set; }
    [LightColumn(Name = "ADDRESS")]
    public string Address { get; set; } = string.Empty;
}