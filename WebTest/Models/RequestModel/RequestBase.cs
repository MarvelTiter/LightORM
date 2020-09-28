using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTest.Models.RequestModel
{
    public class RequestBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
