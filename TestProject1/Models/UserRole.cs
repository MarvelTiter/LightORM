#nullable disable
namespace TestProject1.Models;


[LightTable(Name = "USER_ROLE")]
public class UserRole
{
    [LightColumn(Name = "USER_ID", PrimaryKey = true)]
    public string UserId { get; set; }
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true)]
    public string RoleId { get; set; }
}
