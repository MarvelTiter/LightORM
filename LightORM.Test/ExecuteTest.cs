using LightORM.Test.Models;
using MDbContext;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

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
        public async Task InsertNew()
        {
            var db = SqliteDbContext();
            var p = new Power();
            p.PowerId = "TEST";
            p.PowerName = "Test";
            var list = await db.DbSet.Select<Power>().Count(out var total).QueryAsync();
            System.Console.WriteLine($"total:{total}");
            //db.DbSet.Update<Power>(() => new { p.PowerName, p.PowerType }).Where(p => p.PowerId == "TEST");
            //System.Console.WriteLine(db.DbSet);

            //db.DbSet.Select<Power, Users>();
            //System.Console.WriteLine(db.DbSet);
            //db.DbSet.Update<Power>(() => new { p.PowerName, p.PowerType }).Where(p => p.PowerId == "TEST");
            //System.Console.WriteLine(db.DbSet);
        }
    }
}
