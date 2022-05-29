using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test.Models
{
    public class GeneralReq
    {
        public string Keyword { get; set; }
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime End { get; set; } = DateTime.Now;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int From => (PageIndex - 1) * PageSize;
        public int To => PageIndex * PageSize;
    }
}
