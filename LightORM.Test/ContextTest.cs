using LightORM.Test.Models;
using MDbContext;
using Microsoft.Data.Sqlite;
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

namespace LightORM.Test {
    public class ContextTest {
        [SetUp]
        public void Setup() {

        }

        [Test]
        public void SingleTest() {
            using (var db = GetContext()) {
                db.DbSet.Count<NetConfig>();
                var count = db.Single<int>();
                Assert.IsTrue(count == 4);
            }
        }

        [Test]
        public void AddTest() {
            using (var db = GetContext()) {
                db.DbSet.Select<NetConfig>()
                    .Where(nc => nc.ConfigName == "»ª¹¤");
                NetConfig netConfig = db.Single<NetConfig>();
                netConfig.ConfigName = "»ª¹¤2";
                db.DbSet.Insert(netConfig);
                db.Execute();
            }
        }

        [Test]
        public void IEnumerableTest() {
            var list = netConfigs();
            foreach (var item in list) {
                Console.WriteLine(item.ToString());
            }
        }

        private IEnumerable<Users> netConfigs() {
            using (var db = GetContext()) {
                //var db = GetContext();
                db.DbSet.Select<Users>();
                return db.Query<Users>();
            }
        }


        private DbContext GetContext() {
            //DbContext.Init(DbBaseType.Sqlite);
            //var conn = new SqliteConnection(@"DataSource=E:\GitRepositories\CGS.db");
            //return conn.DbContext();
            DbContext.Init(DbBaseType.SqlServer);
            var conn = new SqlConnection("Data Source=172.20.10.8;Initial Catalog=APDSDB2020;User ID=sa;Password=sa");
            return conn.DbContext();

        }

        [Test]
        public void Test2() {
            using (var db = GetContext()) {
                var list = db.Query<Users>(" SELECT TOP 10 * FROM TBD_XinCheLiangJianCeShuJuJiJieGuoBiao where ShuJuShangChuangBiaoZhi = 0 order by JYJLD_JianCeRiQi desc", null);
                int c = list.Count();
            }
        }


        [Test]
        public void TestNumber() {
            var number = 12312312.123;
            var s = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            var d = Regex.Replace(s, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            var r = Regex.Replace(d, ".", m => "¸ºÔª¿ÕÁãÒ¼·¡ÈþËÁÎéÂ½Æâ°Æ¾Á¿Õ¿Õ¿Õ¿Õ¿Õ¿Õ¿Õ·Ö½ÇÊ°°ÛÇªÍòÒÚÕ×¾©Ûòïöð¦"[m.Value[0] - '-'].ToString());
            Debug.WriteLine(r);
        }

    }
}