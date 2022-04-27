using DExpSql;
using LightORM.Test.Models;
using MDbContext;
using MDbContext.Context.Extension;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test
{
    internal class GroupByRollupTest
    {
        private DbContext VbDbContext()
        {
            DbContext.Init(DbBaseType.Oracle);
            var conn = new OracleConnection("Data Source=192.168.56.11:1521/ORCL;Persist Security Info=True;User ID=CGS;Password=CGS2020");
            return conn.DbContext();
        }
        [Test]
        public async Task RollupTest()
        {
            var db = VbDbContext();
            var dt = await db.DbSet.Select<Jobs>(j => new
            {
                j.StnId,
                C = SqlFn.Count()
            })
                .GroupBy(j => j.StnId, rollup: true).ToDataTableAsync();
            foreach (DataRow item in dt.Rows)
            {
                Console.WriteLine($"{item[0]} - {item[1]}");
            }
        }
    }
}
