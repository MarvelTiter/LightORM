using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTest.Models.Entities
{
	public class JOBS
	{
		/// <summary>
		/// 作业ID（定期检验）
		/// <summary>
		public double JOB_ID { get; set; }
		/// <summary>
		/// 作业流水号
		/// <summary>
		public string JOB_SN { get; set; }
		/// <summary>
		/// 作业顺序号
		/// <summary>
		public double JOB_SEQ { get; set; }
		/// <summary>
		/// 作业创建时间
		/// <summary>
		public DateTime JOB_DATE { get; set; }
		/// <summary>
		/// 号牌号码
		/// <summary>
		public string JOB_PLATE { get; set; }
		/// <summary>
		/// 号牌种类
		/// <summary>
		public string JOB_PCLASS { get; set; }
		/// <summary>
		/// 企业ID
		/// <summary>
		public string STN_ID { get; set; }
		/// <summary>
		/// 作业状态
		/// <summary>
		public double JOB_STATE { get; set; }
		/// <summary>
		/// 当前操作用户ID
		/// <summary>
		public string USR_ID { get; set; }
		/// <summary>
		/// 作业更新时间戳
		/// <summary>
		public DateTime JOB_TIMESTAMP { get; set; }
		/// <summary>
		/// 作业备注
		/// <summary>
		public string JOB_COMMENT { get; set; }
		/// <summary>
		/// PDA照片是否已齐全
		/// <summary>
		public double JOB_PHOTOFLAG { get; set; }
		/// <summary>
		/// PDA查验是否已完成
		/// <summary>
		public double JOB_INSFLAG { get; set; }
		/// <summary>
		/// 环保查验是否已完成
		/// <summary>
		public double JOB_HBINSFLAG { get; set; }
		/// <summary>
		/// 检验类别
		/// <summary>
		public string JOB_JYLB { get; set; }
		/// <summary>
		/// 车辆所属类别
		/// <summary>
		public string JOB_CLSSLB { get; set; }
		/// <summary>
		/// 检验流水号
		/// <summary>
		public string JYLSH { get; set; }
		/// <summary>
		/// 车辆识别代号
		/// <summary>
		public string CLSBDH { get; set; }
		/// <summary>
		/// 检验次数
		/// <summary>
		public double JYCS { get; set; }
		/// <summary>
		/// 保险单开始时间
		/// <summary>
		public DateTime BXDKSSJ { get; set; }
		/// <summary>
		/// 保险单结束时间
		/// <summary>
		public DateTime BXDJSSJ { get; set; }
		/// <summary>
		/// 
		/// <summary>
		public double UPLOADTOCHECK { get; set; }
		/// <summary>
		/// 监管系统流水号
		/// <summary>
		public string LSH { get; set; }
	}

}
