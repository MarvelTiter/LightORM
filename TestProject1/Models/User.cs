﻿#nullable disable
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
    public DateTime LastLogin { get; set; }

    [LightNavigateAttribute(ManyToMany = typeof(UserRole), MainName = nameof(UserId), SubName = nameof(UserRole.UserId))]
    public IEnumerable<Role> UserRoles { get; set; }
}
