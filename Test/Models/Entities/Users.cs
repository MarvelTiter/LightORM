using System;

namespace Test.Models.Entities
{
    public class Users 
    {
        public int ID { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public string LOGINIP { get; set; }
        public string ROLESLIST { get; set; }
        public string DQSJ { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? LOGINDATE { get; set; }
    }

}
