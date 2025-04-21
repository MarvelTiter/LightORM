namespace LightORMTest.Models;


[LightTable(Name = "ROLE_POWER")]
public class RolePower
{
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true)]
    public string RoleId { get; set; }
    [LightColumn(Name = "POWER_ID", PrimaryKey = true)]
    public string PowerId { get; set; }
}
