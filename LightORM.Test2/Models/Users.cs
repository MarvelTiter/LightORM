using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Test2.Models
{
    public class Users
    {
        public string UserName { get; set; } = "Hello";
        public string Password { get; set; } = "12345";
        public string Tel { get; set; } = "13414";
        public string Sex { get; set; } = "男";
        public int? Age { get; set; } = 10;
        public string Duty { get; set; }
        [ColumnName("IS_USE")]
        public bool? IsUse { get; set; }
        public DateTime? ValidDate { get; set; }

        public override string ToString()
        {
            return $"{UserName} => 电话：{Tel} => 性别：{Sex} => 年龄：{Age} => 有效期：{ValidDate}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [TableName("USERS")]
    public class CgsUsers
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [ColumnName("USR_ID")]
        [TableHeader("用户ID", 0)]
        [PrimaryKey]
        public string UsrId { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        [ColumnName("USR_NAME")]
        [TableHeader("用户姓名", 1)]
        public string UsrName { get; set; }

        /// <summary>
        /// 用户身份证号
        /// </summary>
        [ColumnName("USR_IDENTITY")]
        public string UsrIdentity { get; set; }

        /// <summary>
        /// 用户密码（MD5）
        /// </summary>
        [ColumnName("USR_PASS")]
        public string UsrPass { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [ColumnName("DPT_ID")]
        [TableHeader("部门ID", 4)]
        public string DptId { get; set; }

        /// <summary>
        /// 企业ID
        /// </summary>
        [ColumnName("STN_ID")]
        [TableHeader("企业ID", 5)]
        public string StnId { get; set; }

        /// <summary>
        /// 用户组ID
        /// </summary>
        [ColumnName("GRP_ID")]
        [TableHeader("用户组ID", 6)]
        public string GrpId { get; set; }

        /// <summary>
        /// 是否启用（1-是，0-否）
        /// </summary>
        [ColumnName("USR_ENABLE")]
        [TableHeader("是否启用", 7)]
        public YesOrNo UsrEnable { get; set; }

        /// <summary>
        /// 用户有效期
        /// </summary>
        [ColumnName("USR_DEADLINE")]
        [TableHeader("用户有效期", 8)]
        public DateTime UsrDeadline { get; set; }

        /// <summary>
        /// 密码有效期
        /// </summary>
        [ColumnName("PSW_DEADLINE")]
        [TableHeader("密码有效期", 9)]
        public DateTime PswDeadline { get; set; }

        /// <summary>
        /// 是否锁定（1-是，0-否）
        /// </summary>
        [ColumnName("USR_ISLOCK")]
        [TableHeader("是否锁定", 10)]
        public YesOrNo UsrIslock { get; set; }

        /// <summary>
        /// 锁定开始时间
        /// </summary>
        [ColumnName("USR_LOCKTIME")]
        public DateTime UsrLocktime { get; set; }

        /// <summary>
        /// 密码错误次数
        /// </summary>
        [ColumnName("USR_ERRORCOUNT")]
        public int UsrErrorcount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ColumnName("RIGHT")]
        [TableHeader("权限", 13)]
        public string Right { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        [ColumnName("RIGISTRATION")]
        [TableHeader("注册时间", 14)]
        public DateTime Rigistration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ColumnName("REGION_ID")]
        public string RegionId { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [ColumnName("LATEST")]
        [TableHeader("最后登录时间", 16)]
        public DateTime Latest { get; set; }

        /// <summary>
        /// 登录IP
        /// </summary>
        [ColumnName("IP")]
        [TableHeader("登录IP", 17)]
        public string Ip { get; set; }
    }

    public enum YesOrNo
    {
        Yes = 1,
        No = 0,
    }
}
