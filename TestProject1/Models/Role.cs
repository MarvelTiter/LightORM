namespace TestProject1.Models;

[LightTable(Name = "ROLE")]
public class Role
{
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true)]
    public string RoleId { get; set; }
    [LightColumn(Name = "ROLE_NAME")]
    public string RoleName { get; set; }

    [LightNavigate(ManyToMany = typeof(UserRole), MainName = nameof(RoleId), SubName = nameof(UserRole.RoleId))]
    public ICollection<User> Users { get; set; }

    [LightNavigate(ManyToMany =typeof(RolePower), MainName = nameof(RoleId), SubName = nameof(RolePower.RoleId))]
    public ICollection<Power> Powers { get; set; }
}
