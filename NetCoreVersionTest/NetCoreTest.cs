using MDbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;
using Test.Core.Models;
using System.Data.SqlClient;

namespace NetCoreVersionTest {
    [TestClass]
    public class NetCoreTest {
        [TestMethod]
        public void TestMethod1() {
            DbContext.Init(0);
            using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                //using (IDbConnection conn = new SqlConnection("Persist Security Info=False;User ID=sa;Password=sa;Initial Catalog=APDSDB2020;Server=192.168.0.104")) {
                var db = conn.DbContext();
                var sql = db.DbSet.Select<JobFile>(jf => new { jf.JOB_ID, jf.FLT_ID, jf.FLT_CATEGORY });
                var result = db.Query<JobFile>();
                int index = 0;
                foreach (var item in result) {
                    Debug.WriteLine($"Index:{index++} => {item.JOB_ID}:{item.FLT_ID?.Trim()} {item.FLT_CATEGORY}");
                }
            }
        }
    }
}
