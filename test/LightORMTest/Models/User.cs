#nullable disable

namespace LightORMTest.Models;

public enum SignType
{
    None = 0,
    Vip = 1,
    Svip = 2
}

[LightTable(Name = "USER")]
public class User
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [LightColumn(Name = "ID", PrimaryKey = true, AutoIncrement = true, Comment = "自增ID")]
    public int Id { get; set; }
    [LightColumn(Name = "USER_ID", PrimaryKey = true, Comment = "用户ID")]
    public string UserId { get; set; }
    [LightColumn(Name = "USER_NAME", Comment = "名称")]
    public string UserName { get; set; }
    [LightColumn(Name = "PASSWORD", Comment = "密码")]
    public string Password { get; set; }
    [LightColumn(Name = "AGE", Comment = "年龄")]
    public int? Age { get; set; }
    [LightColumn(Name = "SIGN", Comment = "签名")]
    public SignType Sign { get; set; }
    [LightColumn(Name = "LAST_LOGIN", Comment = "最后登录时间")]
    public DateTime? LastLogin { get; set; }
    [LightColumn(Name = "MODIFY_DATE", Comment = "修改时间")]
    public DateTime? ModifyTime { get; set; } 

    [LightColumn(Name = "IS_LOCK", Comment = "是否锁定")]
    public bool? IsLock { get; set; }

    [LightNavigate(ManyToMany = typeof(UserRole), MainName = nameof(UserId), SubName = nameof(UserRole.UserId))]
    public IEnumerable<Role> UserRoles { get; set; }

    [LightNavigate(nameof(Id), nameof(City.Id))]
    public City City { get; set; }

}

public class City
{
    public string Id { get; set; }
    public string Name { get; set; }
}