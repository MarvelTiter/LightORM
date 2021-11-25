
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

        }
        [Test]
        public void SelectTest()
        {
            Watch(sql =>
            {
                sql.Select<Users, Job>().Where<Job>(j => j.BNS_ID > p.Bns);
            });

        }
        [Test]
        public void JoinTest()
        {
            Watch(sql =>
            {
                sql.Select<Users>()
                .InnerJoin<Job>((u, j) => u.Duty == j.CLSBDH);

            });
        }

        private void Watch(Action<ExpressionSql> action)
        {
            ExpressionSql eSql = new ExpressionSql((int)DbBaseType.SqlServer);
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
