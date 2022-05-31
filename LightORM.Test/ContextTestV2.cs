
using LightORM.Test.Models;
using MDbContext;
using MDbContext.NewExpSql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace LightORM.Test
{
    class ContextTestV2
    {
        P p = new P { Age = 5, Bns = 10 };
        [SetUp]
        public void Setup()
        {
            Watch(sql =>
            {

            });
        }
        [Test]
        public void SelectTest()
        {
            Watch(sql =>
            {
                sql.Select<Users, Job>().Where<Job>(j => j.BNS_ID > p.Bns && j.USR_ID == "h").Where(u => u.Age > 5);
            });

        }
        [Test]
        public void JoinTest()
        {
            Watch(sql =>
            {
                sql.Select<Users>().InnerJoin<Job>((u, j) => u.Duty == j.STN_ID).Where<Job>(j => j.BNS_ID > p.Bns && j.USR_ID == "h");

            });
        }

        [Test]
        public void UpdateTest()
        {
            Watch(sql =>
            {
                var u = new Users();
                u.UserName = "Hello";
                u.IsUse = true;
                u.Age = 18;
                sql.Update(u).Where(u => u.Age > 10 && u.Duty == "321").Where(u => u.Sex == "男");
            });
        }

        [Test]
        public void InsertTest()
        {
            Watch(sql =>
            {
                var u = new Users();
                u.UserName = "Hello";
                u.IsUse = true;
                u.Age = 18;
                sql.Insert(u);

            });
        }

        private void Watch(Action<ExpressionSql> action)
        {
            ExpressionSql eSql = new ExpressionSql(DbBaseType.SqlServer);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action(eSql);
            stopwatch.Stop();
            Console.WriteLine($"Cost {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine(eSql);
            Console.WriteLine("====================================");
        }
    }
}
