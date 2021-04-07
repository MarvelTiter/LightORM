using MDbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Diagnostics;

namespace UnitTest {
    [TestClass]
    public class NetFxTest {
        [TestMethod]
        public void TestMethod1() {
            DbContext.Init(1);
            using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                var db = conn.DbContext();
                var sql = db.DbSet.Select<Job>().Paging(0, 10);
                var result = db.Query<Job>();
                foreach (Job item in result) {
                    Debug.WriteLine($"{item.JOB_ID}-{item.JOB_SN}-{item.JOB_COMMENT}");
                }
            }
        }
    }
}
