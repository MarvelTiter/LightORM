using LightORM.Test.Models;
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
        [SetUp]
        public void Setup()
        {

        }
        [Test]
        public void SelectTest()
        {
            NewMethod(5, 5);
            NewMethod(10, 15);

            static void NewMethod(int a1, int a2)
            {
                ExpressionSql eSql = new ExpressionSql(MDbContext.DbBaseType.SqlServer);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                eSql.Select<Users, Job>(distanct: true)
                    .InnerJoin<JobFile>((u, jf) => u.Duty == jf.FLT_CATEGORY)
                .InnerJoin<Job, JobFile>((j, jf) => j.CLSBDH == jf.FLT_ID)
                .Where(u => u.Age > a1)
                .Where<Job>(j => j.BNS_ID > a2);
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                Console.WriteLine(eSql);
            }
        }
    }
}
