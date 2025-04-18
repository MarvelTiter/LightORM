﻿using LightORM.Providers.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.ExpressionTest
{
    [TestClass]
    public class ConditionalTest : TestBase
    {
        [TestMethod]
        public void Test()
        {
            Expression<Func<User, int?>> exp = u => u.Age > 10 ? 1 : null;
            var table = TableContext.GetTableInfo(typeof(User))!;
            var ctx = new ResolveContext(CustomSqlite.Instance, table);
            var result = exp.Resolve(SqlResolveOptions.Select, ctx);
            Console.WriteLine(result.SqlString);
        }
    }
}
