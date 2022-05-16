﻿using LightORM.Test.Models;
using MDbContext;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System.IO;

namespace LightORM.Test
{
    internal class ExecuteTest
    {
        private DbContext SqliteDbContext()
        {
            DbContext.Init(DbBaseType.Sqlite);
            var path = Path.GetFullPath("../../../Demo.db");
            var conn = new SqliteConnection($"DataSource={path}");
            return conn.DbContext();
        }
        [Test]
        public void Exec()
        {
            var db = SqliteDbContext();
            Power power = new Power();
            power.PowerId = "TEST001";
            power.PowerName = "测试001";
            db.DbSet.Insert(power);
            System.Console.WriteLine(db.DbSet);
            db.DbSet.Select<Power>();
            System.Console.WriteLine(db.DbSet);
            var list = db.Query<Power>();
            foreach (var item in list)
            {
                System.Console.WriteLine(item.PowerName);
            }
        }
    }
}
