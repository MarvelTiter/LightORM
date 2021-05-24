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
                //using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                var db = conn.DbContext();
                var sql = db.DbSet.Select<Users>();
                var result = db.Query<Users>();
                foreach (var item in result) {
                    Debug.WriteLine(item.ToString());
                }
            }
        }
    }
}
