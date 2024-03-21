

namespace TestProject1.Models;



[LightTable(Name = "ROLE")]
public class Role
{
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true)]
    public string RoleId { get; set; }
    [LightColumn(Name = "ROLE_NAME")]
    public string RoleName { get; set; }
}
