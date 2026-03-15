namespace LightORMTest.SqlGenerate;

public class UpdateSql_Json : TestBase
{
    [TestMethod]
    public void UpdateJsonField()
    {
        var sql = Db.Update<JsonTestModel>()
            .Set(j => j.Json!.NestJson!.Name, "test")
            .Where(j => j.Id == 5)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void UpdateJsonFieldBySqlFn()
    {
        var sql = Db.Update<JsonTestModel>()
            .Set(j => SqlFn.JsonSet(j.Json!, "$.NestJson.Name", "test"))
            .Where(j => j.Id == 5)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }
}
