using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Core.Models {
    [TableName("DQJY_JOBS")]
    public class Job {
        public double JOB_ID { get; set; }
        public string JOB_SN { get; set; }
        public double JOB_SEQ { get; set; }
        public DateTime? JOB_DATE { get; set; }
        public string JOB_PLATE { get; set; }
        public string JOB_PCLASS { get; set; }
        public double BNS_ID { get; set; }
        public string STN_ID { get; set; }
        public double JOB_STATE { get; set; }
        public string USR_ID { get; set; }
        public DateTime? JOB_TIMESTAMP { get; set; }
        public string JOB_COMMENT { get; set; }
        public string JOB_WHMD { get; set; }
        public string JOB_CANCELRESULT { get; set; }
        public double JOB_APP1_RLT { get; set; }
        public string JOB_APP1_USR_ID { get; set; }
        public double JOB_APP2_RLT { get; set; }
        public string JOB_APP2_USR_ID { get; set; }
        public string BOOKING_SOURCE { get; set; }
        public string CYYXM { get; set; }
        public string JGXTJYLSH { get; set; }
        public string CLSBDH { get; set; }

        public override string ToString() {
            return $"{JOB_ID} => {JOB_COMMENT} => {CLSBDH}";
        }
    }
}
