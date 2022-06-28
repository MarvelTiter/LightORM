using MDbEntity.Attributes;

namespace LightORM.Test2.Models
{
    [Table(Name = "USER")]
    public class User
    {
        [Column(Name = "USER_ID")]
        public string UserId { get; set; }
        [Column(Name = "USER_NAME")]
        public string UserName { get; set; }
        [Column(Name = "PASSWORD")]
        public string Password { get; set; }
    }
}
