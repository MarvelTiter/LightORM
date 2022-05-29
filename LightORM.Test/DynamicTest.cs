﻿using LightORM.Test.Models;
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
        public async Task DynamicResult()
        {
            var db = VbDbContext();
            var result = await db.DbSet.Select<CgsUsers>()
                .Top(10).ToListAsync();
            foreach (var item in result)
            {
                Console.WriteLine($"{item.UsrId}-{item.UsrName}");
            }
        }
    }
}
