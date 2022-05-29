/*
 * 文件由 CodeGenerator 生成
 * 时间：2022-04-14
 * 作者：yaoqinglin
 */

using MDbEntity.Attributes;

namespace LightORM.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TableName("INSPECT_JUDGERESULT")]
    public class InspectJudgeresult
    {

        /// <summary>
        /// 检验流水号
        /// </summary>
        [ColumnName("JYLSH")]
        [TableHeader("检验流水号", 0)]
        public string Jylsh { get; set; }

        /// <summary>
        /// 安检机构编号
        /// </summary>
        [ColumnName("JYJGBH")]
        [TableHeader("安检机构编号", 1)]
        public string Jyjgbh { get; set; }

        /// <summary>
        /// 检测线代号
        /// </summary>
        [ColumnName("JCXDH")]
        [TableHeader("检测线代号", 2)]
        public string Jcxdh { get; set; }

        /// <summary>
        /// 检验次数
        /// </summary>
        [ColumnName("JYCS")]
        [TableHeader("检验次数", 3)]
        public int Jycs { get; set; }

        /// <summary>
        /// 号牌号码
        /// </summary>
        [ColumnName("HPHM")]
        [TableHeader("号牌号码", 4)]
        public string Hphm { get; set; }

        /// <summary>
        /// 号牌种类
        /// </summary>
        [ColumnName("HPZL")]
        [TableHeader("号牌种类", 5)]
        public string Hpzl { get; set; }

        /// <summary>
        /// 车辆识别代号
        /// </summary>
        [ColumnName("CLSBDH")]
        [TableHeader("车辆识别代号", 6)]
        public string Clsbdh { get; set; }

        /// <summary>
        /// 检验结论（合格，不合格）
        /// </summary>
        [ColumnName("JYJL")]
        [TableHeader("检验结论（合格，不合格）", 7)]
        public string Jyjl { get; set; }

        /// <summary>
        /// 授权签字人
        /// </summary>
        [ColumnName("PZRXM")]
        [TableHeader("授权签字人", 8)]
        public string Pzrxm { get; set; }

        /// <summary>
        /// 检验报告建议
        /// </summary>
        [ColumnName("JYBGJY")]
        [TableHeader("检验报告建议", 9)]
        public string Jybgjy { get; set; }

        /// <summary>
        /// 检验报告备注
        /// </summary>
        [ColumnName("JYBGBZ")]
        [TableHeader("检验报告备注", 10)]
        public string Jybgbz { get; set; }

    }
}
