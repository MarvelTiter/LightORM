using MDbEntity.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LightORM.Test2.DemoModel
{
    public enum YesOrNo
    {
        [Display(Name = "是")]
        Yes = 1,
        [Display(Name = "否")]
        No = 0,
    }
    [Table(Name = "USER222")]
    [TableIndex(nameof(UserName), nameof(Password))]
    public class User
    {
        [Column(Name = "ID", AutoIncrement = true, PrimaryKey = true)]
        public long? Id { get; set; }
        [Column(Name = "USER_ID", PrimaryKey = true, Comment = "用户ID")]
        public string UserId { get; set; }
        [Column(Name = "USER_NAME", Default = "TEST", Comment = "用户姓名")]
        public string UserName { get; set; }
        [Column(Name = "PASSWORD", PrimaryKey = true)]
        public string Password { get; set; }
        /// <summary>
        /// 用户是否启用(1-是，0-否）
        /// </summary>
        public YesOrNo Enable { get; set; }
        [MDbEntity.Attributes.Ignore]
        public object Test { get; set; }

        public override string ToString()
        {
            return $"{UserId}-{UserName}";
        }
    }
}
