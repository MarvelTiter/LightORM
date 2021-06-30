using MDbContext.SqlExecutor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;
using LightORM.Test.Models;

namespace LightORM.Test {
    public class CertificateTest {
        Dictionary<Certificate, string> cache;
        [SetUp]
        public void Setup() {
            IDbConnection conn = new SqliteConnection();
            cache = new Dictionary<Certificate, string>();
            var c1 = new Certificate("12321", System.Data.CommandType.Text, conn, typeof(Users), typeof(Job));
            cache.Add(c1, "Hello");
        }

        [Test]
        public void HashCodeTest() {
            IDbConnection conn = new SqliteConnection();
            var c2 = new Certificate("12321", System.Data.CommandType.Text, conn, typeof(Users), typeof(Job));
            cache.TryGetValue(c2, out var v);
            Assert.IsTrue(v == "Hello");
        }
    }
}
