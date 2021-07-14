using LightORM.Test.Models;
using MDbContext.NewExpSql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace LightORM.Test {
    class ContextTestV2 {
        [Test]
        public void SelectTest() {
            NewMethod();
            NewMethod();

            static void NewMethod() {
                ExpressionSql eSql = new ExpressionSql(MDbContext.DbBaseType.SqlServer);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                eSql.Select<Users, Job>((u, j) => new { u.Age, u.Duty, j.BNS_ID }, true)
                    .InnerJoin<Job, JobFile>((j, jf) => j.CLSBDH == jf.FLT_ID)
                    .Where(u => u.Age > 10)
                    .Where<Job>(j => j.BNS_ID > 5);
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                Console.WriteLine(eSql);
            }
        }
    }
}
