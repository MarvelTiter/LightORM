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
            DbContext.Init(1);
            //using (IDbConnection conn = new SqlConnection("Persist Security Info=False;User ID=sa;Password=sa;Initial Catalog=APDSDB2020;Server=192.168.0.104")) {
            using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                    var db = conn.DbContext();
                var sql = db.DbSet.Select<JobFile>();
                var result = db.Query<JobFile>();
                int index = 0;
                foreach (var item in result) {
                    Debug.WriteLine($"Index:{index++} => {item.JOB_ID}:{item.FLT_ID?.Trim()} 长度:{item?.FLT_FILE?.Length}");
                }
            }
        }
    }
}
