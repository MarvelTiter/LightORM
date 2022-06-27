using LightORM.Test.Models;
using MDbContext;
using MDbContext.Context.Extension;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        public async Task DynamicResult()
        {
            var db = VbDbContext();
            var result = await db.DbSet.Select<CgsUsers>().ToListAsync();
            foreach (var item in result)
            {
                //Console.WriteLine($"{item.PowerId}-{item.PowerName}");
                Console.WriteLine($"{item.UsrId}-{item.UsrName}");
            }
        }

        private DbContext SqliteDbContext()
        {
            DbContext.Init(DbBaseType.Sqlite);
            var path = Path.GetFullPath("../../../Demo.db");
            var conn = new SqliteConnection($"DataSource={path}");
            return conn.DbContext();
        }
    }
}
