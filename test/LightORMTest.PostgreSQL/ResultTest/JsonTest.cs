
using LightORMTest.Models;

namespace LightORMTest.PostgreSQL.ResultTest;

[TestClass]
public class JsonTest : LightORMTest.ResultTest.JsonTest
{
    public override DbBaseType DbType => DbBaseType.PostgreSQL;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UsePostgreSQL(ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }

    [TestMethod]
    public override async Task TestJsonUpdateAsync()
    {
        await PrepareJsonDataAsync(Db, async () =>
        {
            await Db.Update<JsonTestModel>().Set(j => SqlFn.JsonSet(j.JsonObject, "{City,Name}", "\"NewName\"")).Where(j => j.Id == 1).ExecuteAsync();

            var updated = await Db.Select<JsonTestModel>().Where(j => j.Id == 1).FirstAsync();
            Assert.IsNotNull(updated);
            Assert.AreEqual("NewName", updated.JsonObject["City"]!["Name"]!.GetValue<string>());

            await Db.Update<JsonTestModel>().Set(j => j.Json!.NestJson!.Name, "\"NewName\"").Where(j => j.Id == 5).ExecuteAsync();

            var nestJsonNameNew = await Db.Select<JsonTestModel>().Where(j => j.Id == 5).ToListAsync(j => j.Json!.NestJson!.Name);

            Assert.HasCount(1, nestJsonNameNew);
            Assert.AreEqual("NewName", nestJsonNameNew[0]);

        });
    }
}