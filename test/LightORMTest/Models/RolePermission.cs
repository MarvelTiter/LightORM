#nullable disable
namespace LightORMTest.Models;


[LightTable(Name = "ROLE_PERMISSION")]
public class RolePermission
{
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true, Comment = "角色ID")]
    public string RoleId { get; set; }
    [LightColumn(Name = "POWER_ID", PrimaryKey = true, Comment = "权限ID")]
    public string PermissionId { get; set; }
}
