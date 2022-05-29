using MDbContext.SqlExecutor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using LightORM.Test.Models;
using Microsoft.Data.Sqlite;

namespace LightORM.Test
{
    public class CertificateTest {
        Dictionary<Certificate, string> cache;
        [SetUp]
        public void Setup() {
            IDbConnection conn = new SqliteConnection();
            cache = new Dictionary<Certificate, string>();
            var c1 = new Certificate(" SELECT TOP 10 * FROM TBD_XinCheLiangJianCeShuJuJiJieGuoBiao where ShuJuShangChuangBiaoZhi = 0 order by JYJLD_JianCeRiQi desc", System.Data.CommandType.Text, conn, typeof(Users), typeof(Job));
            cache.Add(c1, "Hello");
        }

        [Test]
        public void HashCodeTest() {
            IDbConnection conn = new SqliteConnection();
            var c2 = new Certificate(" SELECT TOP 10 * FROM TBD_XinCheLiangJianCeShuJuJiJieGuoBiao where ShuJuShangChuangBiaoZhi = 0 order by JYJLD_JianCeRiQi desc", System.Data.CommandType.Text, conn, typeof(Users), typeof(Job));
            cache.TryGetValue(c2, out var v);
            Assert.IsTrue(v == "Hello");
        }
    }
}
