using LightORM.DbEntity.Attributes;
using System.ComponentModel.DataAnnotations;
#nullable disable
namespace TestProject1.Models;



[LightTable(Name = "USER")]
public class User
{
    [LightColumn(Name = "USER_ID")]
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
    public DateTime? LastLogin { get; set; }
}
