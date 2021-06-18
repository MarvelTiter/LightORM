using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Test.Models {
    public class Users {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Tel { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; }
        public string Duty { get; set; }
        public bool? IsUse { get; set; }
        public DateTime? ValidDate { get; set; }

        public override string ToString() {
            return $"{UserName} => 电话：{Tel} => 性别：{Sex} => 年龄：{Age} => 有效期：{ValidDate}";
        }
    }
}
