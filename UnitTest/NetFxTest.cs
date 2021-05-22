using MDbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Test.Core.Models;

namespace UnitTest {
    [TestClass]
    public class NetFxTest {
        [TestMethod]
        public void TestMethod1() {
            DbContext.Init(0);
            using (IDbConnection conn = new SqlConnection("Persist Security Info=False;User ID=sa;Password=sa;Initial Catalog=APDSDB2020;Server=192.168.0.104")) {
                var db = conn.DbContext();
                var sql = db.DbSet.Select<Users>().Paging(0, 10);
                var result = db.Query<Users>();
                foreach (Users item in result) {
                    Debug.WriteLine(item?.ToString());
                }
            }
        }
    }
}
