using LightORM.Test.Models;
using MDbContext;
using MDbContext.Context.Extension;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test
{
    internal class DynamicTest
    {
        private DbContext VbDbContext()
        {
            DbContext.Init(DbBaseType.Oracle);
            var conn = new OracleConnection("Data Source=192.168.56.11:1521/ORCL;Persist Security Info=True;User ID=CGS;Password=CGS2020");
            return conn.DbContext();
        }
        [Test]
        public void DynamicResult()
        {
            //var db = VbDbContext();
            //db.DbSet.Select<CgsUsers>()
            //    .Top(10).ToListAsync();
        }
    }
}
