#nullable disable
namespace LightORMTest.Models;

[LightTable(Name = "ROLE")]
public class Role
{
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true, Comment = "角色ID")]
    public string RoleId { get; set; }
    [LightColumn(Name = "ROLE_NAME", Comment = "角色名称")]
    public string RoleName { get; set; }

    [LightNavigate(ManyToMany = typeof(UserRole), MainName = nameof(RoleId), SubName = nameof(UserRole.RoleId))]
    public IEnumerable<User> Users { get; set; }

    [LightNavigate(ManyToMany = typeof(RolePermission), MainName = nameof(RoleId), SubName = nameof(RolePermission.RoleId))]
    public IEnumerable<Permission> Powers { get; set; } = [];
}
