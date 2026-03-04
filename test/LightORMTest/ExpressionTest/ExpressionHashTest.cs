using LightORM.Utils.Vistors;
using System.Linq.Expressions;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class ExpressionHashTest
{
    [TestMethod]
    public void TypeTest()
    {
        Expression<Func<User, bool>> exp1 = u => u.Id > 10;
        Expression<Func<Product, bool>> exp2 = u => u.Id > 10;
        var id1 = ExpressionHasher.Default.ComputeHash64(exp1);
        var id2= ExpressionHasher.Default.ComputeHash64(exp2);
        Assert.AreNotEqual(id1, id2);
    }
}