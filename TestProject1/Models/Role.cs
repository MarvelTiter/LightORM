﻿namespace TestProject1.Models;

[LightTable(Name = "ROLE")]
public class Role
{
    [LightColumn(Name = "ROLE_ID", PrimaryKey = true)]
    public string RoleId { get; set; }
    [LightColumn(Name = "ROLE_NAME")]
    public string RoleName { get; set; }

    [LightNavigateAttribute(ManyToMany = typeof(UserRole), MainName = nameof(RoleId), SubName = nameof(UserRole.RoleId))]
    public IEnumerable<User> Users { get; set; }

    [LightNavigateAttribute(ManyToMany =typeof(RolePower), MainName = nameof(RoleId), SubName = nameof(RolePower.RoleId))]
    public IEnumerable<Power> Powers { get; set; }
}
