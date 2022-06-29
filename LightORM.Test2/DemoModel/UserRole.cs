using MDbEntity.Attributes;

namespace LightORM.Test2.DemoModel
{
    [TableName("USER_ROLE")]
    [Table(Name = "USER_ROLE")]
    public class UserRole
    {
        [ColumnName("USER_ID")]
        [Column(Name = "USER_ID")]
        public string UserId { get; set; }
        [ColumnName("ROLE_ID")]
        [Column(Name = "ROLE_ID")]
        public string RoleId { get; set; }
    }
}
