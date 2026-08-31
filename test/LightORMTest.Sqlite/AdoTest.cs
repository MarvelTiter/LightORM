using LightORMTest.Models;

namespace LightORMTest.Sqlite;

[TestClass]
public class AdoTest : LightORMTest.AdoTest
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite(ConnectString.Value);
        option.UseSqlite("Memory", "Data Source=:memory:;Version=3;New=True;");
        option.UseInterceptor<LightOrmAop>();
    }


    [TestMethod]
    public void TransientContextTest()
    {
        var ctx = Db.SwitchDatabase("Memory");
        var str = ctx.Ado.Connection.Connection.ConnectionString;
        Console.WriteLine(str);
    }
}
