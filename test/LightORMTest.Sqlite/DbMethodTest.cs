using LightORMTest.Models;

namespace LightORMTest.Sqlite;

[TestClass]
public class DbMethodTest : LightORMTest.DbMethodTest
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite("Test",ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }

    [TestMethod]
    public void SelectWithAttr()
    {
        var scoped = Db.CreateScoped();
        scoped.SelectWithAttr<User>().ToList();
        Db.SelectWithAttr<User>();
    }
    
}
