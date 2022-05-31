/*
 * 文件由 CodeGenerator 生成
 * 时间：2022-04-07
 * 作者：yaoqinglin
 */

using MDbEntity.Attributes;
using System;

namespace LightORM.Test2.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TableName("JOBS")]
    public class Jobs
    {

        /// <summary>
        /// 作业ID（定期检验）
        /// </summary>
        [ColumnName("JOB_ID")]
        public int JobId { get; set; }

        /// <summary>
        /// 作业流水号
        /// </summary>
        [ColumnName("JOB_SN")]
        public string JobSn { get; set; }

        /// <summary>
        /// 作业顺序号
        /// </summary>
        [ColumnName("JOB_SEQ")]
        public int JobSeq { get; set; }

        /// <summary>
        /// 作业创建时间
        /// </summary>
        [ColumnName("JOB_DATE")]
        public DateTime JobDate { get; set; }

        /// <summary>
        /// 号牌号码
        /// </summary>
        [ColumnName("JOB_PLATE")]
        public string JobPlate { get; set; }

        /// <summary>
        /// 号牌种类
        /// </summary>
        [ColumnName("JOB_PCLASS")]
        public string JobPclass { get; set; }

        /// <summary>
        /// 企业ID
        /// </summary>
        [ColumnName("STN_ID")]
        public string StnId { get; set; }

        /// <summary>
        /// 作业状态
        /// </summary>
        [ColumnName("JOB_STATE")]
        public int JobState { get; set; }

        /// <summary>
        /// 当前操作用户ID
        /// </summary>
        [ColumnName("USR_ID")]
        public string UsrId { get; set; }

        /// <summary>
        /// 作业备注
        /// </summary>
        [ColumnName("JOB_COMMENT")]
        public string JobComment { get; set; }

        /// <summary>
        /// PDA照片是否已齐全
        /// </summary>
        [ColumnName("JOB_PHOTOFLAG")]
        public int JobPhotoflag { get; set; }

        /// <summary>
        /// PDA查验是否已完成
        /// </summary>
        [ColumnName("JOB_INSFLAG")]
        public int JobInsflag { get; set; }

        /// <summary>
        /// 环保查验是否已完成
        /// </summary>
        [ColumnName("JOB_HBINSFLAG")]
        public int JobHbinsflag { get; set; }

        /// <summary>
        /// 检验类别
        /// </summary>
        [ColumnName("JOB_JYLB")]
        public string JobJylb { get; set; }

        /// <summary>
        /// 车辆所属类别
        /// </summary>
        [ColumnName("JOB_CLSSLB")]
        public string JobClsslb { get; set; }

        /// <summary>
        /// 检验流水号
        /// </summary>
        [ColumnName("JYLSH")]
        public string Jylsh { get; set; }

        /// <summary>
        /// 车辆识别代号
        /// </summary>
        [ColumnName("CLSBDH")]
        public string Clsbdh { get; set; }

        /// <summary>
        /// 检验次数
        /// </summary>
        [ColumnName("JYCS")]
        public int Jycs { get; set; }

        /// <summary>
        /// 保险单开始时间
        /// </summary>
        [ColumnName("BXDKSSJ")]
        public DateTime Bxdkssj { get; set; }

        /// <summary>
        /// 保险单结束时间
        /// </summary>
        [ColumnName("BXDJSSJ")]
        public DateTime Bxdjssj { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ColumnName("UPLOADTOCHECK")]
        public int Uploadtocheck { get; set; }

        /// <summary>
        /// 监管系统流水号
        /// </summary>
        [ColumnName("LSH")]
        public string Lsh { get; set; }

    }
}
