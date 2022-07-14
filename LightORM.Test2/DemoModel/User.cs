using MDbEntity.Attributes;

namespace LightORM.Test2.DemoModel
{
    [Table(Name = "USER")]
    public class User
    {
        [Column(Name = "USER_ID", PrimaryKey = true)]
        public string UserId { get; set; }
        [Column(Name = "USER_NAME")]
        public string UserName { get; set; }
        [Column(Name = "PASSWORD")]
        [MDbEntity.Attributes.Ignore]
        public string Password { get; set; }
    }
}
