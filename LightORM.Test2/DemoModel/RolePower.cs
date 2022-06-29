using MDbEntity.Attributes;

namespace LightORM.Test2.DemoModel
{
    [Table(Name = "ROLE_POWER")]
    [TableName("ROLE_POWER")]
    public class RolePower
    {
        [Column(Name = "ROLE_ID")]
        [ColumnName("ROLE_ID")]
        public string RoleId { get; set; }
        [Column(Name = "POWER_ID")]
        [ColumnName("POWER_ID")]
        public string PowerId { get; set; }
    }
}
