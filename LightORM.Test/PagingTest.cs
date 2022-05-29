using LightORM.Test.Models;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace LightORM.Test
{
    public class PagingTest
    {
        [Test]
        public void SqlServerPaging()
        {
            var db = new MDbContext.DbContext(MDbContext.DbBaseType.SqlServer, new SqlConnection());
            db.DbSet.Select<CgsUsers>()
                .Paging(1, 20);
            System.Console.WriteLine(db.DbSet);
        }

        [Test]
        public void OraclePaging()
        {
            var db = new MDbContext.DbContext(MDbContext.DbBaseType.Oracle, new OracleConnection());
            db.DbSet.Select<CgsUsers>()
                .Paging(1, 20);
            System.Console.WriteLine(db.DbSet);
        }
    }
}