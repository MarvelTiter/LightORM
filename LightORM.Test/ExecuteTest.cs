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
        [Test]
        public void Exec2()
        {
            var db = SqliteDbContext();
            var p = new Power();
            db.DbSet.Insert(p);
            System.Console.WriteLine(db.DbSet);
            db.DbSet.Select<Power>();
            System.Console.WriteLine(db.DbSet);
            db.DbSet.Update<Power>(p);
            System.Console.WriteLine(db.DbSet);
        }

        [Test]
        public void InsertNew()
        {
            var db = SqliteDbContext();
            var p = new Power();
            p.PowerId = "TEST";
            p.PowerName = "Test";
            db.DbSet.Update<Power>(() => new { p.PowerName, p.PowerType }).Where(p => p.PowerId == "TEST");
            System.Console.WriteLine(db.DbSet);

            db.DbSet.Select<Power, Users>();
            System.Console.WriteLine(db.DbSet);
            db.DbSet.Update<Power>(() => new { p.PowerName, p.PowerType }).Where(p => p.PowerId == "TEST");
            System.Console.WriteLine(db.DbSet);
        }
    }
}