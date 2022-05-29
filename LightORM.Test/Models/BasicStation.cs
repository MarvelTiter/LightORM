/*
 * 文件由 CodeGenerator 生成
 * 时间：2022-04-07
 * 作者：yaoqinglin
 */

using MDbEntity.Attributes;
using System;

namespace LightORM.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TableName("BASIC_STATION")]
    public class BasicStation
    {

        /// <summary>
        /// 
        /// </summary>
        [ColumnName("REGION_ID")]
        public string RegionId { get; set; }

        /// <summary>
        /// 检测站坐标Y
        /// </summary>
        [ColumnName("LOCATIONY")]
        public string Locationy { get; set; }

        /// <summary>
        /// 检测站坐标X
        /// </summary>
        [ColumnName("LOCATIONX")]
        public string Locationx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ColumnName("ZNSH")]
        public int Znsh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ColumnName("JCZJC")]
        public string Jczjc { get; set; }

        /// <summary>
        /// 接口序列号
        /// </summary>
        [ColumnName("JKXLH")]
        public string Jkxlh { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        [ColumnName("DWMC")]
        public string Dwmc { get; set; }

        /// <summary>
        /// 单位代码
        /// </summary>
        [ColumnName("DWJGDM")]
        public string Dwjgdm { get; set; }

        /// <summary>
        /// 应用场景编号
        /// </summary>
        [ColumnName("CJSQBH")]
        public string Cjsqbh { get; set; }

        /// <summary>
        /// 路试驻车坡道数量
        /// </summary>
        [ColumnName("LSZCS")]
        public int Lszcs { get; set; }

        /// <summary>
        /// 路试车道数
        /// </summary>
        [ColumnName("LSCDS")]
        public int Lscds { get; set; }

        /// <summary>
        /// 底盘动态车道数
        /// </summary>
        [ColumnName("DTCDS")]
        public int Dtcds { get; set; }

        /// <summary>
        /// 检测线数量
        /// </summary>
        [ColumnName("JCXSL")]
        public int Jcxsl { get; set; }

        /// <summary>
        /// 录像端口
        /// </summary>
        [ColumnName("LXPORT")]
        public string Lxport { get; set; }

        /// <summary>
        /// 录像IP
        /// </summary>
        [ColumnName("LXIP")]
        public string Lxip { get; set; }

        /// <summary>
        /// 检测站代码（预约使用）
        /// </summary>
        [ColumnName("STN_CODE")]
        public string StnCode { get; set; }

        /// <summary>
        /// 日常联系人联系电话
        /// </summary>
        [ColumnName("RCLXRLXDH")]
        public string Rclxrlxdh { get; set; }

        /// <summary>
        /// 日常联系人身份证号
        /// </summary>
        [ColumnName("RCLXRSFZH")]
        public string Rclxrsfzh { get; set; }

        /// <summary>
        /// 日常联系人
        /// </summary>
        [ColumnName("RCLXR")]
        public string Rclxr { get; set; }

        /// <summary>
        /// 负责人联系电话
        /// </summary>
        [ColumnName("FZRLXDH")]
        public string Fzrlxdh { get; set; }

        /// <summary>
        /// 负责人身份证号
        /// </summary>
        [ColumnName("FZRSFZH")]
        public string Fzrsfzh { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        [ColumnName("FZR")]
        public string Fzr { get; set; }

        /// <summary>
        /// 法人代表联系电话
        /// </summary>
        [ColumnName("FRDBLXDH")]
        public string Frdblxdh { get; set; }

        /// <summary>
        /// 法人代表身份证号
        /// </summary>
        [ColumnName("FRDBSFZH")]
        public string Frdbsfzh { get; set; }

        /// <summary>
        /// 法人代表
        /// </summary>
        [ColumnName("FRDB")]
        public string Frdb { get; set; }

        /// <summary>
        /// 资格许可发放单位
        /// </summary>
        [ColumnName("RDSFFDW")]
        public string Rdsffdw { get; set; }

        /// <summary>
        /// 许可检验范围（00-大型车，01-小型车，02-许可路试超检验能力的车，03-上门检验）
        /// </summary>
        [ColumnName("XKJYFW")]
        public string Xkjyfw { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [ColumnName("YZBM")]
        public string Yzbm { get; set; }

        /// <summary>
        /// 单位地址
        /// </summary>
        [ColumnName("DWDZ")]
        public string Dwdz { get; set; }

        /// <summary>
        /// 暂停原因
        /// </summary>
        [ColumnName("ZTYY")]
        public string Ztyy { get; set; }

        /// <summary>
        /// 状态标记（0-撤销，1-正常，2-停用，3-首次备案申请，4-过有效期）
        /// </summary>
        [ColumnName("ZT")]
        public string Zt { get; set; }

        /// <summary>
        /// 审核意见
        /// </summary>
        [ColumnName("SHYJ")]
        public string Shyj { get; set; }

        /// <summary>
        /// 使用管理部门
        /// </summary>
        [ColumnName("SYGLBM")]
        public string Syglbm { get; set; }

        /// <summary>
        /// 审核标记（0-待审核，1-同意，2-不同意）
        /// </summary>
        [ColumnName("SHBJ")]
        public string Shbj { get; set; }

        /// <summary>
        /// 实际日检测能力(摩托辆)
        /// </summary>
        [ColumnName("SHIJIRJCMTSL")]
        public int Shijirjcmtsl { get; set; }

        /// <summary>
        /// 设计日检测能力(摩托辆)
        /// </summary>
        [ColumnName("SHEJIRJCMTSL")]
        public int Shejirjcmtsl { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [ColumnName("BZ")]
        public string Bz { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        [ColumnName("GXRQ")]
        public DateTime Gxrq { get; set; }

        /// <summary>
        /// 管理部门
        /// </summary>
        [ColumnName("GLBM")]
        public string Glbm { get; set; }

        /// <summary>
        /// 发证机关
        /// </summary>
        [ColumnName("FZJG")]
        public string Fzjg { get; set; }

        /// <summary>
        /// 未通过省级质检部门考核人数
        /// </summary>
        [ColumnName("WTGSZJBMKHRS")]
        public int Wtgszjbmkhrs { get; set; }

        /// <summary>
        /// 通过省级质检部门考核人数
        /// </summary>
        [ColumnName("TGSZJBMKHRS")]
        public int Gszjbmkhrs { get; set; }

        /// <summary>
        /// 其他工位人数
        /// </summary>
        [ColumnName("QTGWRS")]
        public int Qtgwrs { get; set; }

        /// <summary>
        /// 总检工位人数
        /// </summary>
        [ColumnName("ZJGWRS")]
        public int Zjgwrs { get; set; }

        /// <summary>
        /// 底盘工位人数
        /// </summary>
        [ColumnName("DPGWRS")]
        public int Dpgwrs { get; set; }

        /// <summary>
        /// 引车员人数
        /// </summary>
        [ColumnName("YCYRS")]
        public int Ycyrs { get; set; }

        /// <summary>
        /// 录入工位人数
        /// </summary>
        [ColumnName("LRGWRS")]
        public int Lrgwrs { get; set; }

        /// <summary>
        /// 外检工位人数
        /// </summary>
        [ColumnName("WJGWRS")]
        public int Wjgwrs { get; set; }

        /// <summary>
        /// 检测人员总数
        /// </summary>
        [ColumnName("JCRYZS")]
        public int Jcryzs { get; set; }

        /// <summary>
        /// 实际日检测能力(汽车辆)
        /// </summary>
        [ColumnName("SHIJIRJCNL")]
        public int Shijirjcnl { get; set; }

        /// <summary>
        /// 设计日检测能力(汽车辆)
        /// </summary>
        [ColumnName("SHEJIRJCNL")]
        public int Shejirjcnl { get; set; }

        /// <summary>
        /// 资格许可有效期止
        /// </summary>
        [ColumnName("RDYXQZ")]
        public DateTime Rdyxqz { get; set; }

        /// <summary>
        /// 资格许可有效期始
        /// </summary>
        [ColumnName("RDYXQS")]
        public DateTime Rdyxqs { get; set; }

        /// <summary>
        /// 资格许可证书编号
        /// </summary>
        [ColumnName("RDSBH")]
        public string Rdsbh { get; set; }

        /// <summary>
        /// 是否与公安网联网（1-是，2-否）
        /// </summary>
        [ColumnName("SFLW")]
        public string Sflw { get; set; }

        /// <summary>
        /// 安检机构名称
        /// </summary>
        [ColumnName("JCZMC")]
        public string Jczmc { get; set; }

        /// <summary>
        /// 检测站编号
        /// </summary>
        [ColumnName("JCZBH")]
        public string Jczbh { get; set; }

    }
}
