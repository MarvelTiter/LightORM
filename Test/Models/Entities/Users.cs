using System;

namespace Test.Models.Entities {
    public class Users {
        public string Account { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Tel { get; set; }
        public string Sex { get; set; }
        public int Age { get; set; }
        public string Duty { get; set; }
        public bool IsUse { get; set; }
        public DateTime? ValidDate { get; set; }
    }

}
