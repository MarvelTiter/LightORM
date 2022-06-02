using DExpSql;
using LightORM.Test.Models;
using MDbContext;
using MDbContext.Context.Extension;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
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
                    .Where(nc => nc.ConfigName == "����");
                NetConfig netConfig = db.Single<NetConfig>();
                netConfig.ConfigName = "����2";
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
            var r = Regex.Replace(d, ".", m => "��Ԫ����Ҽ��������½��ƾ��տտտտտտշֽ�ʰ��Ǫ�����׾������"[m.Value[0] - '-'].ToString());
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
                BH = s.Jczbh,
                s.Jczmc,
                HeFaWeiShenHe = SqlFn.Sum(() => j.JobState == 4),
                ZhengZaiHeFaShenHe = SqlFn.Sum(() => j.JobState == 5),
                HeFaShenHeTongGuo = SqlFn.Sum(() => j.JobState == 6),
                HeFaShenHeBuTongGuo = SqlFn.Sum(() => j.JobState == 7),
                DaiDaYin = SqlFn.Sum(() => j.JobState == 8),
                YiDaYin = SqlFn.Sum(() => j.JobState == 9),
                YiQuXiao = SqlFn.Sum(() => j.JobState == 20)
            })
                .InnerJoin<BasicStation>((j, s) => j.StnId == s.Jczbh && j.Jycs == 1)
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

        private async Task<IEnumerable<BasicStation>> local()
        {
            var db = VbDbContext();
            return await db.DbSet.Select<BasicStation>().ToListAsync<BasicStation>();
        }

        class temp
        {
            public string jczbh { get; set; }
            public string jczmc { get; set; }
            public long lrs { get; set; }
            public long cys { get; set; }
            public long cytgs { get; set; }
            public long cybtgs { get; set; }
        }

        [Test]
        public async Task TestCount()
        {
            var db = VbDbContext();
            var req = new GeneralReq();
            req.Start = new DateTime(2018, 4, 1);
            req.End = new DateTime(2018, 5, 5);
            var dt = await db.DbSet.Select<InspectItemdataF1Xxxx, InspectJudgeresult, InspectLoginInfo, BasicStation>((f1, judge, login, stn) => new
            {
                stn.Jczbh,
                stn.Jczmc,
                LRS = SqlFn.Sum(() => stn.Jczbh),
                CYS = SqlFn.Sum(() => judge.Jyjl != null),
                CYTGS = SqlFn.Sum(() => judge.Jyjl == "�ϸ�"),
                CYBTGS = SqlFn.Sum(() => judge.Jyjl == "���ϸ�")
            })
                 .InnerJoin<InspectJudgeresult>((f1, judge) => f1.Jylsh == judge.Jylsh && f1.Jycs == judge.Jycs)
                 .InnerJoin<InspectLoginInfo>((f1, info) => f1.Jylsh == info.Jylsh && f1.Jycs == info.Jycs)
                 .InnerJoin<BasicStation>((f1, s) => f1.Jyjgbh == s.Jczbh)
                 .Where<InspectLoginInfo>(i => i.Dlsj >= req.Start && i.Dlsj < req.End)
                 .IfWhere(!string.IsNullOrEmpty(req.Keyword), f => f.Jyjgbh == req.Keyword)
                 .GroupBy<BasicStation>((s) => new { s.Jczbh, s.Jczmc })
                 .ToListAsync<temp>();
        }
    }
}