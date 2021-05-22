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
