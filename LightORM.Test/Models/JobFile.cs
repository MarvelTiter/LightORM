using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Test.Models {
	[TableName("DQJY_JOBFILES")]
	public class JobFile {
		public DateTime? JFL_DATE { get; set; }
		public int JFL_NEED { get; set; }
		public string JFL_REMARK { get; set; }
		public DateTime? JFL_MODIFYTIME { get; set; }
		public byte[] FLT_FILE { get; set; }
		public long JOB_ID { get; set; }
		public string FLT_ID { get; set; }
		public int FILE_ID { get; set; }
		public string FLT_CATEGORY { get; set; }
		public int JFL_EXIST { get; set; }
		public int? JFL_OK { get; set; }
		public int? JFL_NO { get; set; }
	}
}
