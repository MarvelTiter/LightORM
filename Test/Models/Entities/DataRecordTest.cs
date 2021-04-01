using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models.Entities {
    public class DataRecordTest {
        public int IntCol { get; set; }
        public int? NullableIntCol { get; set; }
        public bool BooleanCol { get; set; }
        public bool? NullableBooleanCol { get; set; }
        public decimal DecimalCol { get; set; }
        public decimal? NullableDecimalCol { get; set; }
        public DateTime DatetimeCol { get; set; }
        public DateTime? NullableDatetimeCol { get; set; }
        public string StringCol { get; set; }
    }
}
