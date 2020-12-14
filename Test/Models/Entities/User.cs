using MDbEntity.Attributes;
using System;
using System.Collections.Generic;

namespace Test.Models.Entities {
    [TableName("t_user")]
    public class User {
        private string _yhbh;
        [PrimaryKey]
        /// <summary>
        /// 用户编号
        /// </summary>
        public string YHBH {
            get { return _yhbh; }
            set { _yhbh = value; }
        }

        private string _yhmc;
        /// <summary>
        /// 用户名称
        /// </summary>
        public string YHMC {
            get { return _yhmc; }
            set { _yhmc = value; }
        }

        private string _yhmm;
        /// <summary>
        /// 用户密码
        /// </summary>
        public string YHMM {
            get { return _yhmm; }
            set { _yhmm = value; }
        }

        private DateTime _yhmmsj;
        /// <summary>
        /// 用户密码有效期
        /// </summary>
        public DateTime YHMMSJ {
            get { return _yhmmsj; }
            set { _yhmmsj = value; }
        }

        private string _yhzbh;
        /// <summary>
        /// 用户组编号
        /// </summary>
        public string YHZBH {
            get { return _yhzbh; }
            set { _yhzbh = value; }
        }

        private string _yhjsbh;
        /// <summary>
        /// 用户角色编号
        /// </summary>
        public string YHJSBH {
            get { return _yhjsbh; }
            set { _yhjsbh = value; }
        }

        private string _jyjgbh;
        /// <summary>
        /// 机构编号
        /// </summary>
        public string JYJGBH {
            get { return _jyjgbh; }
            set { _jyjgbh = value; }
        }

        private string _yhjb;
        /// <summary>
        /// 用户级别
        /// </summary>
        public string YHJB {
            get { return _yhjb; }
            set { _yhjb = value; }
        }

        private string _yhsfqy;
        /// <summary>
        /// 用户是否启用(1-是，0-否）
        /// </summary>
        public string YHSFQY {
            get { return _yhsfqy; }
            set { _yhsfqy = value; }
        }

        private string _yhsfzh;
        /// <summary>
        /// 用户身份证号
        /// </summary>
        public string YHSFZH {
            get { return _yhsfzh; }
            set { _yhsfzh = value; }
        }

        private string _yhlxdh;
        /// <summary>
        /// 用户联系电话
        /// </summary>
        public string YHLXDH {
            get { return _yhlxdh; }
            set { _yhlxdh = value; }
        }

        private string _yhyddh;
        /// <summary>
        /// 用户移动电话
        /// </summary>
        public string YHYDDH {
            get { return _yhyddh; }
            set { _yhyddh = value; }
        }

        private DateTime _yhcjsj;
        /// <summary>
        /// 用户创建时间
        /// </summary>
        public DateTime YHCJSJ {
            get { return _yhcjsj; }
            set { _yhcjsj = value; }
        }

        private string _sfgly;
        /// <summary>
        /// 是否管理员用户（默认否）
        /// </summary>
        public string SFGLY {
            get { return _sfgly; }
            set { _sfgly = value; }
        }

        private string _yhjsz;
        /// <summary>
        /// 用户角色组
        /// </summary>
        public string YHJSZ {
            get { return _yhjsz; }
            set { _yhjsz = value; }
        }

        private int _mmcwcs;
        /// <summary>
        /// 密码错误次数
        /// </summary>
        public int MMCWCS {
            get { return _mmcwcs; }
            set { _mmcwcs = value; }
        }

        private DateTime _yhsdsj;
        /// <summary>
        /// 用户锁定时间
        /// </summary>
        public DateTime YHSDSJ {
            get { return _yhsdsj; }
            set { _yhsdsj = value; }
        }

        private bool _yhsfccdl;
        public bool YHSFCCDL {
            get { return _yhsfccdl; }
            set { _yhsfccdl = value; }
        }
        //最后登录IP
        public string ZHDLIP { get; set; }
        //最后登录时间
        public DateTime ZHDLSJ { get; set; }
    }

    public class UserEx : User {
        public List<string> Roles { get; set; }

        public string UserRoles { get; set; }
    }

}
