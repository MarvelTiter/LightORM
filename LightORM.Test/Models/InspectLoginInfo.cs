/*
 * 文件由 CodeGenerator 生成
 * 时间：2022-04-14
 * 作者：yaoqinglin
 */

using MDbEntity.Attributes;
using System;

namespace LightORM.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TableName("INSPECT_LOGIN_INFO")]
    public class InspectLoginInfo
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
        /// 检验次数
        /// </summary>
        [ColumnName("JYCS")]
        [TableHeader("检验次数", 2)]
        public int Jycs { get; set; }

        /// <summary>
        /// 检测线代号
        /// </summary>
        [ColumnName("JCXDH")]
        [TableHeader("检测线代号", 3)]
        public string Jcxdh { get; set; }

        /// <summary>
        /// 检验项目
        /// </summary>
        [ColumnName("JYXM")]
        [TableHeader("检验项目", 4)]
        public string Jyxm { get; set; }

        /// <summary>
        /// 登录时间
        /// </summary>
        [ColumnName("DLSJ")]
        [TableHeader("登录时间", 5)]
        public DateTime Dlsj { get; set; }

        /// <summary>
        /// 登录员
        /// </summary>
        [ColumnName("DLY")]
        [TableHeader("登录员", 6)]
        public string Dly { get; set; }

        /// <summary>
        /// 引车员
        /// </summary>
        [ColumnName("YCY")]
        [TableHeader("引车员", 7)]
        public string Ycy { get; set; }

        /// <summary>
        /// 外检员
        /// </summary>
        [ColumnName("WJY")]
        [TableHeader("外检员", 8)]
        public string Wjy { get; set; }

        /// <summary>
        /// 动态检验员
        /// </summary>
        [ColumnName("DTJYY")]
        [TableHeader("动态检验员", 9)]
        public string Dtjyy { get; set; }

        /// <summary>
        /// 底盘检验员
        /// </summary>
        [ColumnName("DPJYY")]
        [TableHeader("底盘检验员", 10)]
        public string Dpjyy { get; set; }

        /// <summary>
        /// 登录员（身份证号）
        /// </summary>
        [ColumnName("DLYSFZH")]
        [TableHeader("登录员（身份证号）", 11)]
        public string Dlysfzh { get; set; }

        /// <summary>
        /// 引车员（身份证号）
        /// </summary>
        [ColumnName("YCYSFZH")]
        [TableHeader("引车员（身份证号）", 12)]
        public string Ycysfzh { get; set; }

        /// <summary>
        /// 外检员（身份证号）
        /// </summary>
        [ColumnName("WJYSFZH")]
        [TableHeader("外检员（身份证号）", 13)]
        public string Wjysfzh { get; set; }

        /// <summary>
        /// 动态检验员（身份证号）
        /// </summary>
        [ColumnName("DTJYYSFZH")]
        [TableHeader("动态检验员（身份证号）", 14)]
        public string Dtjyysfzh { get; set; }

        /// <summary>
        /// 底盘检验员（身份证号）
        /// </summary>
        [ColumnName("DPJYYSFZH")]
        [TableHeader("底盘检验员（身份证号）", 15)]
        public string Dpjyysfzh { get; set; }

        /// <summary>
        /// 送检人（姓名）
        /// </summary>
        [ColumnName("SJR")]
        [TableHeader("送检人（姓名）", 16)]
        public string Sjr { get; set; }

        /// <summary>
        /// 送检人身份证号
        /// </summary>
        [ColumnName("SJRSFZH")]
        [TableHeader("送检人身份证号", 17)]
        public string Sjrsfzh { get; set; }

        /// <summary>
        /// 检验过程开始时间
        /// </summary>
        [ColumnName("KSSJ")]
        [TableHeader("检验过程开始时间", 18)]
        public DateTime Kssj { get; set; }

        /// <summary>
        /// 检验过程结束时间
        /// </summary>
        [ColumnName("JSSJ")]
        [TableHeader("检验过程结束时间", 19)]
        public DateTime Jssj { get; set; }

        /// <summary>
        /// 检验状态（1-车辆登录，2-正在检测，3-检测完成，4-取消检测，5-取消登录）
        /// </summary>
        [ColumnName("JYZT")]
        [TableHeader("检验状态（1-车辆登录，2-正在检测，3-检测完成，4-取消检测，5-取消登录）", 20)]
        public string Jyzt { get; set; }

        /// <summary>
        /// 正在检验项目
        /// </summary>
        [ColumnName("ZZJYXM")]
        [TableHeader("正在检验项目", 21)]
        public string Zzjyxm { get; set; }

        /// <summary>
        /// 报告单数据是否上传（1-是；0-否）
        /// </summary>
        [ColumnName("DATA_ISUPLOAD")]
        [TableHeader("报告单数据是否上传（1-是；0-否）", 22)]
        public string DataIsupload { get; set; }

        /// <summary>
        /// 数据上传时间
        /// </summary>
        [ColumnName("DATA_UPLOAD_DATE")]
        [TableHeader("数据上传时间", 23)]
        public DateTime DataUploadDate { get; set; }

    }
}
