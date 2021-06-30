using LightORM.Test.Models;
using MDbContext;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;
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


        private DbContext GetContext() {
            DbContext.Init(DbBaseType.Sqlite);
            var conn = new SqliteConnection(@"DataSource=E:\GitRepositories\CGS.db");
            return conn.DbContext();
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