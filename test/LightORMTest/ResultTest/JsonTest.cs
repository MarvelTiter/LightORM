using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace LightORMTest.ResultTest;

public class JsonTest : TestBase
{
    private static JsonTestModel CreateJsonModel(int i)
    {
        var jsonString = $$"""
            {
                "Id": {{i}},
                "Name": "Hello{{i}}",
                "Age": {{18 + i}},
                "Role": "Admin{{i}}",
                "Values": [{{1 * i}}, {{2 * i}}, {{3 * i}}],
                "City": {
                    "Name": "Dongguan{{i}}"
                }
            }
            """;
        var testJson = new JsonData()
        {
            Name = $"World{i}",
            Value = 20 + i,
            NestArray = [new() { Name = $"Nest Array{i}", Value = 30 + i }],
            NestList = [new() { Name = $"Nest List{i}", Value = 40 + i }],
            NestJson = new() { Name = $"Nest Object{i}", Value = 50 + i }
        };
        return new JsonTestModel()
        {
            Id = i,
            Json = testJson,
            JsonArray = [testJson],
            JsonList = [testJson],
            JsonObject = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(jsonString)!
        };
    }
    protected static async Task PrepareJsonDataAsync(IExpressionContext db, Func<Task> action)
    {
        db.Delete<JsonTestModel>().FullDelete().Execute();
        await db.Insert(CreateJsonModel(1)).ExecuteAsync();
        await db.Insert(CreateJsonModel(2)).ExecuteAsync();
        await db.Insert(CreateJsonModel(3)).ExecuteAsync();
        await db.Insert(CreateJsonModel(4)).ExecuteAsync();
        await db.Insert(CreateJsonModel(5)).ExecuteAsync();
        await action();
        //db.Delete<JsonTestModel>().FullDelete().Execute();
    }

    [TestMethod]
    public async Task CreateTable()
    {
        await Db.CreateMainDbScoped().CreateTableAsync<JsonTestModel>();
    }

    [TestMethod]
    public async Task TestJsonSelectAsync()
    {
        await PrepareJsonDataAsync(Db, async () =>
        {
            var model = await Db.Select<JsonTestModel>().Where(j => j.Json!.Value == 22).ToListAsync();
            Assert.HasCount(1, model);
            Assert.IsNotNull(model[0].Json);
            Assert.AreEqual("World2", model[0].Json!.Name);

            var list = await Db.Select<JsonTestModel>().Where(j => j.Json!.Value == 22).ToListAsync(j => new
            {
                j.Id,
                j.Json,
            });
            Assert.HasCount(1, list);
            Assert.IsNotNull(list[0].Json);
            Assert.AreEqual("World2", list[0].Json!.Name);

            var dto = await Db.Select<JsonTestModel>().Where(j => j.Json!.Value == 22).ToListAsync(j => new JsonTestModelDto()
            {
                Id = j.Id,
                Json = j.Json,
            });
            Assert.HasCount(1, dto);
            Assert.IsNotNull(dto[0].Json);
            Assert.AreEqual("World2", dto[0].Json!.Name);

            var jsonObject = await Db.Select<JsonTestModel>().Where(j => j.JsonObject["City"]!["Name"]!.GetValue<string>() == "Dongguan3").FirstAsync();
            Assert.IsNotNull(jsonObject);
            Assert.AreEqual(21, jsonObject.JsonObject["Age"]!.GetValue<int>());

            var jsonObjectArr = await Db.Select<JsonTestModel>().Where(j => j.JsonObject["Values"]![1]!.GetValue<int>() == 8).FirstAsync();
            Assert.IsNotNull(jsonObjectArr);
            Assert.AreEqual(22, jsonObjectArr.JsonObject["Age"]!.GetValue<int>());

            var nestJsonName = await Db.Select<JsonTestModel>().Where(j => j.Id == 5).ToListAsync(j => j.Json!.NestJson!.Name);

            Assert.HasCount(1, nestJsonName);
            Assert.AreEqual("Nest Object5", nestJsonName[0]);

        });
    }

    [TestMethod]
    public virtual async Task TestJsonUpdateAsync()
    {
        await PrepareJsonDataAsync(Db, async () =>
        {
            await Db.Update<JsonTestModel>().Set(j => SqlFn.JsonSet(j.JsonObject, "$.City.Name", "NewName"))
            .Where(j => j.Id == 1).ExecuteAsync();

            var updated = await Db.Select<JsonTestModel>().Where(j => j.Id == 1).FirstAsync();
            Assert.IsNotNull(updated);
            Assert.AreEqual("NewName", updated.JsonObject["City"]!["Name"]!.GetValue<string>());

            await Db.Update<JsonTestModel>().Set(j => j.Json!.NestJson!.Name, "NewName")
            .Where(j => j.Id == 5).ExecuteAsync();

            var nestJsonNameNew = await Db.Select<JsonTestModel>().Where(j => j.Id == 5).ToListAsync(j => j.Json!.NestJson!.Name);

            Assert.HasCount(1, nestJsonNameNew);
            Assert.AreEqual("NewName", nestJsonNameNew[0]);

        });
    }
}
