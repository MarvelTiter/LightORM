using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models.Entities
{
    public class SQLLOG
    {
        public string ACTION { get; set; }
        public string SQL { get; set; }
        public string PARAMETER { get; set; }
        public string DESCRIPTION { get; set; }
    }
}
