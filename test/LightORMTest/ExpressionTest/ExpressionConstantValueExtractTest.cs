using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ExpressionVisitTest;

[TestClass]
public class ExpressionConstantValueExtractTest
{
    [TestMethod]
    public void TestExpressionConstraintValue_Variable()
    {
        int value = 10;
        Expression<Func<User, bool>> exp = u => u.Age > value;
        var extract = ExpressionValueExtract.Default.Extract(exp);
        foreach (var item in extract)
        {
            Console.WriteLine($"{item.Name} - {item.Value} - {item.Type}");
        }
    }

    [TestMethod]
    public void TestExpressionConstraintValue_ArrayAccess()
    {
        int[] value = [10];
        Expression<Func<User, bool>> exp = u => u.Age > value[0];
        var extract = ExpressionValueExtract.Default.Extract(exp);
        foreach (var item in extract)
        {
            Console.WriteLine($"{item.Name} - {item.Value} - {item.Type}");
        }
    }
}
