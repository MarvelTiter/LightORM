using DExpSql;
using LightORM.Test.Models;
using MDbContext;
using MDbContext.Context.Extension;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightORM.Test
{
    public class ContextTest
    {
        [SetUp]
        public void Setup()
        {

        }


        [Test]
        public void AddTest()
        {
            using (var db = GetContext())
            {
                db.DbSet.Select<NetConfig>()
                    .Where(nc => nc.ConfigName == "»ª¹¤");
                NetConfig netConfig = db.Single<NetConfig>();
                netConfig.ConfigName = "»ª¹¤2";
                db.DbSet.Insert(netConfig);
                db.Execute();
            }
        }

        [Test]
        public void IEnumerableTest()
        {
            var list = netConfigs();
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }
        }

        private IEnumerable<Users> netConfigs()
        {
            using (var db = GetContext())
            {
                //var db = GetContext();
                db.DbSet.Select<Users>();

                return db.Query<Users>();
            }
        }


        private DbContext GetContext()
        {
            //DbContext.Init(DbBaseType.Sqlite);
            //var conn = new SqliteConnection(@"DataSource=E:\GitRepositories\CGS.db");
            //return conn.DbContext();
            DbContext.Init(DbBaseType.SqlServer);
            var conn = new MySqlConnection("Data Source=172.18.180.41;Database=videocollection;User ID=videocollection;Password=hgbanner;charset=gbk");
            return conn.DbContext();

        }

        [Test]
        public void TestSelect()
        {
            var db = GetContext();
            db.DbSet.Count<Users>()
                .InnerJoin<Job>((j, u) => j.Duty == u.CLSBDH);
            Console.WriteLine(db.DbSet);
        }

        [Test]
        public void TestIn()
        {
            var db = GetContext();
            int[] arr = { 1, 2, 3, 4 };
            db.DbSet.Update<Users>(() => new { Age = 1 })
                .Where(u => u.Age.In(1, 2, 3, 4));
            Console.WriteLine(db.DbSet);
        }

        [Test]
        public void TestWhere()
        {
            var db = GetContext();
            int[] arr = { 1, 2, 3, 4 };
            var d = DateTime.Now.ToString("yyyyMM");
            db.DbSet.Select<Users>()
                .Where(u => u.Duty == d);
        }
        [Test]
        public void TestIfWhere()
        {
            var db = GetContext();
            int[] arr = { 1, 2, 3, 4 };
            var d = DateTime.Now.ToString("yyyyMM");
            db.DbSet.Select<Users>()
                .InnerJoin<Job>((u, j) => u.Age == j.JOB_ID)
                .Where(u => u.Duty.Like("123"))
                .IfWhere(() => arr.Length > 5, u => u.Age > 10)
                .IfWhere(() => arr.Length > 2, u => u.Age > 10)
                .IfWhere(() => arr.Length > 3, u => u.Age > 10);
            Console.WriteLine(db.DbSet);
        }

        [Test]
        public void TestNumber()
        {
            var number = 12312312.123;
            var s = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            var d = Regex.Replace(s, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            var r = Regex.Replace(d, ".", m => "¸ºÔª¿ÕÁãÒ¼·¡ÈþËÁÎéÂ½Æâ°Æ¾Á¿Õ¿Õ¿Õ¿Õ¿Õ¿Õ¿Õ·Ö½ÇÊ°°ÛÇªÍòÒÚÕ×¾©Ûòïöð¦"[m.Value[0] - '-'].ToString());
            Debug.WriteLine(r);
        }

        class p
        {
            public int Age { get; set; } = 11;
            public string Name { get; set; } = "Hello";
        }

        [Test]
        public void TestEntityParameter()
        {
            var db = GetContext();
            var dt = db.QueryDataTable("select * from t_station_user where sptd=?Age", new p());
            Console.WriteLine(dt.Rows.Count);
        }

        [Test]
        public void TestWhere123()
        {
            var db = GetContext();
            db.DbSet.Select<Users>()
                .Where(u => 1 == 1)
                .Where(u => u.UserName.Like(null));
            Console.WriteLine(db.DbSet);
        }

        [Test]
        public void TestSelectFn()
        {
            var db = GetContext();
            var req = new GeneralReq();
            db.DbSet.Select<Jobs, BasicStation>((j, s) => new
            {
                s.Jczbh,
                s.Jczmc,
                HeFaWeiShenHe = Fn.Sum(() => j.JobState == 4),
                ZhengZaiHeFaShenHe = Fn.Sum(() => j.JobState == 5),
                HeFaShenHeTongGuo = Fn.Sum(() => j.JobState == 6),
                HeFaShenHeBuTongGuo = Fn.Sum(() => j.JobState == 7),
                DaiDaYin = Fn.Sum(() => j.JobState == 8),
                YiDaYin = Fn.Sum(() => j.JobState == 9),
                YiQuXiao = Fn.Sum(() => j.JobState == 20)
            })
                .InnerJoin<BasicStation>((j, s) => j.StnId == s.Jczbh)
                .IfWhere(() => !string.IsNullOrEmpty(req.Keyword), j => j.StnId == req.Keyword)
                .Where(j => j.JobDate > req.Start && j.JobDate < req.End)
                .GroupBy<BasicStation>(s => new { s.Jczbh, s.Jczmc });

            db.DbSet.Select<Jobs, BasicStation>((j, s) => new
            {
                s.Jczbh,
                s.Jczmc,
                HeFaWeiShenHe = Fn.Sum(() => j.JobState == 4),
                ZhengZaiHeFaShenHe = Fn.Sum(() => j.JobState == 5),
                HeFaShenHeTongGuo = Fn.Sum(() => j.JobState == 6),
                HeFaShenHeBuTongGuo = Fn.Sum(() => j.JobState == 7),
                DaiDaYin = Fn.Sum(() => j.JobState == 8),
                YiDaYin = Fn.Sum(() => j.JobState == 9),
                YiQuXiao = Fn.Sum(() => j.JobState == 20)
            })
                .InnerJoin<BasicStation>((j, s) => j.StnId == s.Jczbh)
                .IfWhere(() => !string.IsNullOrEmpty(req.Keyword), j => j.StnId == req.Keyword)
                .Where(j => j.JobDate > req.Start && j.JobDate < req.End)
                .GroupBy<BasicStation>(s => new { s.Jczbh, s.Jczmc });

            Console.WriteLine(db.DbSet);
        }



        private DbContext VbDbContext()
        {
            DbContext.Init(DbBaseType.Oracle);
            var conn = new OracleConnection("Data Source=192.168.56.11:1521/ORCL;Persist Security Info=True;User ID=CGS;Password=CGS2020");
            return conn.DbContext();
        }
        [Test]
        public void TestExtension()
        {
            var list = local().Result;
            foreach (var b in list)
            {
                Console.WriteLine(b.Jczmc);
            }
        }

        private Task<IEnumerable<BasicStation>> local()
        {
            var db = VbDbContext();
            return db.DbSet.Select<BasicStation>().ToListAsync<BasicStation>();
        }
    }
}