#nullable disable

namespace LightORMTest.Models;


[LightTable(Name = "USER_ROLE")]
public class UserRole
{
    [LightColumn(Name = "USER_ID", PrimaryKey = true, Comment = "用户ID")]
    public string UserId { get; set; }
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true, Comment = "角色ID")]
    public string RoleId { get; set; }

    [LightNavigate(nameof(UserId), nameof(User.UserId))]
    public User User { get; set; }

    [LightNavigate(nameof(RoleId), nameof(Role.RoleId))]
    public Role Role { get; set; }
}
