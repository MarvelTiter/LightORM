using LightORM.Test.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LightORM.Test {
    public class ExpressionToStringTest {
        [Test]
        public void Test1() {
            Expression expression = buildExp<Users>();
            Console.WriteLine(expression.ToString());
        }

        Expression buildExp<T>() {
            Expression<Func<T, object>> expression = t => t.GetType();
            return expression;
        }
    }
}
