using System.Text.Json.Nodes;

namespace LightORMTest.SqlGenerate;

public class InsertSql_Json : TestBase
{
    [TestMethod]
    public void InsertJsonModel()
    {
        var jsonString = """
            {
                "Name": "Hello",
                "Age": 18,
                "Role": "Admin",
                "Values": [1, 2, 3],
                "City": {
                    "Name": "Dongguan"
                }
            }
            """;
        var testJson = new JsonData()
        {
            Name = "World",
            Value = 20,
            NestArray = [new() { Name = "Nest Array", Value = 30 }],
            NestList = [new() { Name = "Nest List", Value = 40 }],
            NestJson = new() { Name = "Nest Object", Value = 50 }
        };
        var entity = new JsonTestModel()
        {
            Json = testJson,
            JsonArray = [testJson],
            JsonList = [testJson],
            JsonObject = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(jsonString)!
        };
        var sql = Db.Insert(entity).ToSqlWithParameters();
        Console.WriteLine(sql);
    }
}