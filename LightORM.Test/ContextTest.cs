using LightORM.Test.Models;
using MDbContext;
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
        public void TestMethod1() {
            DbContext.Init(0);
            //using (IDbConnection conn = new SqlConnection("Persist Security Info=False;User ID=sa;Password=sa;Initial Catalog=APDSDB2020;Server=192.168.0.104")) {
            using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                var db = conn.DbContext();
                var sql = db.DbSet.Select<Job>();
                var result = db.Query<Job>();
                foreach (var item in result) {
                    Debug.WriteLine(item.ToString());
                }
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