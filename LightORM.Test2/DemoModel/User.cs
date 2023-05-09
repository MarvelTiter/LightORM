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
    [Table(Name = "USER")]
    public class User
    {
        [Column(Name = "USER_ID", PrimaryKey = true, AutoIncrement = true)]
        public string UserId { get; set; }
        [Column(Name = "USER_NAME")]
        public string UserName { get; set; }
        [Column(Name = "PASSWORD")]
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
