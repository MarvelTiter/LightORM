using System.Text.Json.Nodes;

namespace LightORMTest.ResultTest;

/// <summary>
/// ExecutionTest 的 json 列用例分部：以 <see cref="JsonExecTestModel"/> 覆盖原 <see cref="JsonTest"/> 已验证的全部 json 场景
/// (json 值 Where / JsonObject 字符串与数组索引 Where / json 投影与深层投影 / SqlFn.JsonSet 更新 / 嵌套导航路径 Set 更新)，
/// 并额外覆盖 JsonTest 未覆盖的整列 json Set 替换。
/// 表(JSON_EXEC_TEST)与基线数据由 InitData.cs 的 InitDatas 统一 drop+create+seed，测试方法不再自建表。
/// 说明: json 更新能力因库而异(如 Sqlite 不支持 json 更新)，与 JsonTest 行为一致，不支持的库直接抛异常即可。
/// </summary>
public partial class ExecutionTest
{
    private static JsonData CreateNestedJsonData(int id)
    {
        return new JsonData()
        {
            Name = $"World{id}",
            Value = 20 + id,
            NestArray = [new() { Name = $"Nest Array{id}", Value = 30 + id }],
            NestList = [new() { Name = $"Nest List{id}", Value = 40 + id }],
            NestJson = new() { Name = $"Nest Object{id}", Value = 50 + id }
        };
    }

    private static JsonExecTestModel CreateJsonExecModel(int id)
    {
        var nested = CreateNestedJsonData(id);
        return new JsonExecTestModel()
        {
            Id = id,
            Data = nested,
            Arr = [nested, new() { Name = $"Second{id}", Value = 60 + id }],
            Lst = [new() { Name = $"List{id}", Value = 70 + id }],
            // 语义对齐 JsonTest 的 JsonObject 文档: 含 City(嵌套)/Values(数组)/Age/Role, 供字符串索引、数组索引、JsonSet 场景使用
            Obj = new JsonObject
            {
                ["City"] = new JsonObject { ["Name"] = $"Dongguan{id}" },
                ["Values"] = new JsonArray { id, id * 2, id * 3 },
                ["Age"] = 18 + id,
                ["Role"] = $"Admin{id}"
            }
        };
    }

    /// <summary>
    /// 由 InitDatas 调用：清空 JSON_EXEC_TEST 后写入固定基线行(Id=100/101/102)。
    /// </summary>
    private async Task InitJsonDataAsync()
    {
        await Db.Insert(CreateJsonExecModel(100)).TagWith("初始化数据").ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert(CreateJsonExecModel(101)).TagWith("初始化数据").ExecuteAsync(TestContext.CancellationToken);
        await Db.Insert(CreateJsonExecModel(102)).TagWith("初始化数据").ExecuteAsync(TestContext.CancellationToken);
    }

    /// <summary>
    /// Insert 整列写 json → Select 整实体读回，断言四类 json 列(含深层嵌套与 Obj 文档)完整往返。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_Insert_ReadBack()
    {
        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNotNull(m.Data);
        Assert.AreEqual("World100", m.Data!.Name);
        Assert.AreEqual(120, m.Data.Value);
        Assert.IsNotNull(m.Data.NestJson);
        Assert.AreEqual("Nest Object100", m.Data.NestJson!.Name);
        Assert.IsNotNull(m.Arr);
        Assert.HasCount(2, m.Arr!);
        Assert.AreEqual("Second100", m.Arr![1].Name);
        Assert.IsNotNull(m.Lst);
        Assert.HasCount(1, m.Lst!);
        Assert.AreEqual("List100", m.Lst![0].Name);
        Assert.AreEqual("Dongguan100", m.Obj["City"]!["Name"]!.GetValue<string>());
        Assert.AreEqual(118, m.Obj["Age"]!.GetValue<int>());
    }

    /// <summary>
    /// json 对象列标量值作 Where（对应 JsonTest: Where(j.Json!.Value == …)）。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_JsonValue_Where()
    {
        var model = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Data!.Value == 121)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(1, model);
        Assert.IsNotNull(model[0].Data);
        Assert.AreEqual("World101", model[0].Data!.Name);
        Assert.AreEqual(101, model[0].Id);
    }

    /// <summary>
    /// JsonObject 字符串索引与数组索引作 Where（对应 JsonTest: j.JsonObject["City"]["Name"] / j.JsonObject["Values"][1]）。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_JsonObject_IndexWhere()
    {
        // 字符串索引
        var byCity = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Obj["City"]!["Name"]!.GetValue<string>() == "Dongguan102")
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(byCity);
        Assert.AreEqual(102, byCity!.Id);
        Assert.AreEqual(120, byCity.Obj["Age"]!.GetValue<int>());

        // 数组索引 (Values[1] = id*2)
        var byValueArr = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Obj["Values"]![1]!.GetValue<int>() == 202)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(byValueArr);
        Assert.AreEqual(101, byValueArr!.Id);
        Assert.AreEqual(119, byValueArr.Obj["Age"]!.GetValue<int>());
    }

    /// <summary>
    /// json 投影：整 json 对象投影到匿名类型，以及 json 深层导航字段投影（对应 JsonTest 的投影用例）。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_Projection()
    {
        // 投影整 json 对象(匿名)
        var list = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 101)
            .ToListAsync(j => new { j.Id, j.Data }, TestContext.CancellationToken);
        Assert.HasCount(1, list);
        Assert.IsNotNull(list[0].Data);
        Assert.AreEqual("World101", list[0].Data!.Name);

        // 投影 json 深层字段 (Data.NestJson.Name)
        var nestName = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToListAsync(j => j.Data!.NestJson!.Name, TestContext.CancellationToken);
        Assert.HasCount(1, nestName);
        Assert.AreEqual("Nest Object100", nestName[0]);
    }

    /// <summary>
    /// SqlFn.JsonSet 更新 JsonObject 嵌套路径（对应 JsonTest: Set(j => SqlFn.JsonSet(j.JsonObject, "$.City.Name", "NewName"))）。
    /// PostgreSQL 的 JsonSet 路径/值格式与其它库不同，按 DbType 分支处理。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_JsonSet_Update()
    {
        if (DbType == DbBaseType.PostgreSQL)
        {
            await Db.Update<JsonExecTestModel>()
                .Set(j => SqlFn.JsonSet(j.Obj, "{City,Name}", "\"NewName\""))
                .Where(j => j.Id == 100)
                .ExecuteAsync(TestContext.CancellationToken);
        }
        else
        {
            await Db.Update<JsonExecTestModel>()
                .Set(j => SqlFn.JsonSet(j.Obj, "$.City.Name", "NewName"))
                .Where(j => j.Id == 100)
                .ExecuteAsync(TestContext.CancellationToken);
        }

        var updated = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(updated);
        Assert.AreEqual("NewName", updated!.Obj["City"]!["Name"]!.GetValue<string>());
    }

    /// <summary>
    /// 嵌套导航路径 Set 更新强类型 json 子字段（对应 JsonTest: Set(j => j.Json!.NestJson!.Name, "NewName")）。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_NestedPath_Update()
    {
        await Db.Update<JsonExecTestModel>()
            .Set(j => j.Data!.NestJson!.Name, "NewNest")
            .Where(j => j.Id == 101)
            .ExecuteAsync(TestContext.CancellationToken);

        var updated = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 101)
            .ToListAsync(j => j.Data!.NestJson!.Name, TestContext.CancellationToken);
        Assert.HasCount(1, updated);
        Assert.AreEqual("NewNest", updated[0]);
    }

    /// <summary>
    /// 路径 Set 写入<b>复合值</b>(对象)：验证方言按 JSON 解析方式写入而非当字符串字面量
    /// (Sqlite <c>JSON(@p)</c> / SqlServer <c>JSON_QUERY(@p)</c> / PG <c>@p::JSONB</c>)，
    /// 读回应仍是对象而非被序列化后的 JSON 字符串。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_CompositePath_Set_Update()
    {
        await Db.Update<JsonExecTestModel>()
            .Set(j => j.Data!.NestJson, new JsonData { Name = "CompositeNest", Value = 77 })
            .Where(j => j.Id == 101)
            .ExecuteAsync(TestContext.CancellationToken);

        var updated = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 101)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(updated);
        Assert.IsNotNull(updated!.Data!.NestJson);
        Assert.AreEqual("CompositeNest", updated.Data.NestJson!.Name);
        Assert.AreEqual(77, updated.Data.NestJson.Value);
        // 同列其它路径与其它 json 列不受影响
        Assert.AreEqual("World101", updated.Data.Name);
        Assert.IsNotNull(updated.Arr);
    }

    /// <summary>
    /// 路径 Set 的值类型覆盖：数字标量与数组索引路径(字符串)写回的往返验证。
    /// 参数值统一按 JSON 文本序列化(如 4321 / "ArrItem1")，方言须按 JSON 解析后再写入，
    /// 否则会被当成普通字符串字面量导致双重编码(读回多一层引号)。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_ScalarAndIndexPath_Set_Update()
    {
        await Db.Update<JsonExecTestModel>()
            .Set(j => j.Data!.Value, 4321)
            .Set(j => j.Arr![1].Name, "ArrItem1")
            .Where(j => j.Id == 102)
            .ExecuteAsync(TestContext.CancellationToken);

        var updated = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 102)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(updated);
        Assert.AreEqual(4321, updated!.Data!.Value);
        Assert.AreEqual("ArrItem1", updated.Arr![1].Name);
        // 未触及的字段/列保持不变
        Assert.AreEqual("World102", updated.Data.Name);
        Assert.AreEqual("Dongguan102", updated.Obj["City"]!["Name"]!.GetValue<string>());
    }

    /// <summary>
    /// Update 整列替换 json(对象/JsonObject/List)：JsonTest 未覆盖，作为 ExecutionTest json 的补充覆盖。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_WholeColumn_Update()
    {
        var newData = new JsonData() { Name = "Replaced", Value = 999 };
        var newObj = new JsonObject { ["Name"] = "ReplacedNode", ["Age"] = 1 };
        await Db.Update<JsonExecTestModel>()
            .Set(j => j.Data, newData)
            .Set(j => j.Obj, newObj)
            .Set(j => j.Lst, new List<JsonData>() { new() { Name = "OnlyItem", Value = 1 } })
            .Where(j => j.Id == 100)
            .ExecuteAsync(TestContext.CancellationToken);

        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNotNull(m.Data);
        Assert.AreEqual("Replaced", m.Data!.Name);
        Assert.AreEqual(999, m.Data.Value);
        Assert.IsNull(m.Data.NestJson);
        Assert.AreEqual("ReplacedNode", m.Obj["Name"]!.GetValue<string>());
        Assert.IsNotNull(m.Lst);
        Assert.HasCount(1, m.Lst!);
        Assert.AreEqual("OnlyItem", m.Lst![0].Name);
        // 未被更新的列应保持不变
        Assert.IsNotNull(m.Arr);
        Assert.HasCount(2, m.Arr!);
    }

    /// <summary>
    /// 整列赋值必须能覆盖 NULL 现状：先 SetNull 把 json 列置空, 再整列写回对象。
    /// 该场景用于约束方言不得用"路径函数"实现整列更新(如 MySQL 的
    /// <c>JSON_SET(col,'$',val)</c> 在首参为 NULL 时整体返回 NULL, 会静默写不进值),
    /// 须与 Sqlite/SqlServer/Dameng/PG 一样走列直接赋值。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_WholeColumn_Update_FromNull()
    {
        // 1) 先把基线行 101 的 Data 置 NULL
        await Db.Update<JsonExecTestModel>()
            .SetNull(j => j.Data)
            .Where(j => j.Id == 101)
            .ExecuteAsync(TestContext.CancellationToken);

        var nulled = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 101)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(nulled);
        Assert.IsNull(nulled!.Data);

        // 2) 由 NULL 整列写回对象, 必须真正落库
        await Db.Update<JsonExecTestModel>()
            .Set(j => j.Data, new JsonData() { Name = "FromNull", Value = 5 })
            .Where(j => j.Id == 101)
            .ExecuteAsync(TestContext.CancellationToken);

        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 101)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNotNull(m!.Data);
        Assert.AreEqual("FromNull", m.Data!.Name);
        Assert.AreEqual(5, m.Data.Value);
        // 其它 json 列不受影响
        Assert.IsNotNull(m.Arr);
        Assert.HasCount(2, m.Arr!);
        Assert.AreEqual("Dongguan101", m.Obj["City"]!["Name"]!.GetValue<string>());
    }

    /// <summary>
    /// UpdateColumns 指定更新的 json 列(整列引用, 无索引/无成员路径)：值取自更新实体,
    /// 未列入的列必须保持不变。对应 UpdateBuilder.HandleResult 中 AdditionalParameter 为 null 的 Update 分支。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_UpdateColumns_Update()
    {
        // 基于基线行 102 构造更新实体, 仅指定 Data 列参与更新
        var entity = CreateJsonExecModel(102);
        entity.Data = new JsonData() { Name = "ColsData", Value = 777 };
        entity.Obj = new JsonObject { ["Name"] = "ColsNode", ["Age"] = 5 };
        await Db.Update(entity)
            .UpdateColumns(j => j.Data)
            .ExecuteAsync(TestContext.CancellationToken);

        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 102)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNotNull(m.Data);
        Assert.AreEqual("ColsData", m.Data!.Name);
        Assert.AreEqual(777, m.Data.Value);
        // 未列入更新列的 Obj/Arr/Lst 应保持基线值
        Assert.AreEqual("Dongguan102", m.Obj["City"]!["Name"]!.GetValue<string>());
        Assert.IsNotNull(m.Arr);
        Assert.HasCount(2, m.Arr!);
        Assert.IsNotNull(m.Lst);
        Assert.AreEqual("List102", m.Lst![0].Name);
    }

    /// <summary>
    /// UpdateColumns 一次指定多个 json 列(整列引用)：验证多成员场景下每列都能正确生成赋值与参数绑定。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_UpdateColumns_MultiColumns_Update()
    {
        // 基于基线行 101 构造更新实体, Data 与 Obj 两列同时参与更新
        var entity = CreateJsonExecModel(101);
        entity.Data = new JsonData() { Name = "MultiData", Value = 111 };
        entity.Obj = new JsonObject { ["Name"] = "MultiNode", ["Age"] = 7 };
        await Db.Update(entity)
            .UpdateColumns(j => new { j.Data, j.Obj })
            .ExecuteAsync(TestContext.CancellationToken);

        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 101)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNotNull(m.Data);
        Assert.AreEqual("MultiData", m.Data!.Name);
        Assert.AreEqual(111, m.Data.Value);
        Assert.AreEqual("MultiNode", m.Obj["Name"]!.GetValue<string>());
        Assert.AreEqual(7, m.Obj["Age"]!.GetValue<int>());
        // 未列入更新列的 Arr/Lst 应保持基线值
        Assert.IsNotNull(m.Arr);
        Assert.HasCount(2, m.Arr!);
        Assert.IsNotNull(m.Lst);
        Assert.AreEqual("List101", m.Lst![0].Name);
    }

    /// <summary>
    /// SetNull(整列引用) 将 json 列置 NULL：走 SetNullMembers 分支, 生成 col = NULL。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_SetNull_Update()
    {
        await Db.Update<JsonExecTestModel>()
            .SetNull(j => j.Data)
            .Where(j => j.Id == 102)
            .ExecuteAsync(TestContext.CancellationToken);

        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 102)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNull(m.Data);
        // 其它 json 列不受影响
        Assert.IsNotNull(m.Arr);
        Assert.AreEqual("Dongguan102", m.Obj["City"]!["Name"]!.GetValue<string>());
    }

    /// <summary>
    /// Set(json 列, null) 显式赋 null：与 SetNull 等价, 同样落到 SetNullMembers 分支。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_SetNullValue_Update()
    {
        await Db.Update<JsonExecTestModel>()
            .Set(j => j.Data, null)
            .Where(j => j.Id == 102)
            .ExecuteAsync(TestContext.CancellationToken);

        var m = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 102)
            .FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(m);
        Assert.IsNull(m.Data);
        Assert.AreEqual("Dongguan102", m.Obj["City"]!["Name"]!.GetValue<string>());
    }

    /// <summary>
    /// 批量 Insert json 列：一次插入多行, 每行 json 参数独立(Data_0/Data_1...),
    /// 断言多行 json 整列写入均可完整读回。批量场景下 json 列只可能是"整列赋值"。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_BatchInsert_ReadBack()
    {
        var rows = new[] { CreateJsonExecModel(200), CreateJsonExecModel(201) };
        var effect = await Db.Insert(rows)
            .TagWith("批量json插入")
            .ExecuteAsync(TestContext.CancellationToken);
        Assert.AreEqual(2, effect);

        var list = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 200 || j.Id == 201)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, list);

        var m200 = list.Single(j => j.Id == 200);
        Assert.IsNotNull(m200.Data);
        Assert.AreEqual("World200", m200.Data!.Name);
        Assert.AreEqual(220, m200.Data.Value);
        Assert.AreEqual("Nest Object200", m200.Data.NestJson!.Name);
        Assert.IsNotNull(m200.Arr);
        Assert.HasCount(2, m200.Arr!);
        Assert.AreEqual("Second200", m200.Arr![1].Name);
        Assert.IsNotNull(m200.Lst);
        Assert.AreEqual("List200", m200.Lst![0].Name);
        Assert.AreEqual("Dongguan200", m200.Obj["City"]!["Name"]!.GetValue<string>());
        Assert.AreEqual(218, m200.Obj["Age"]!.GetValue<int>());

        var m201 = list.Single(j => j.Id == 201);
        Assert.IsNotNull(m201.Data);
        Assert.AreEqual("World201", m201.Data!.Name);
        Assert.AreEqual(221, m201.Data.Value);
        // 每行参数不得串号: 201 行必须是自己的文档
        Assert.AreEqual("Dongguan201", m201.Obj["City"]!["Name"]!.GetValue<string>());
        Assert.IsNotNull(m201.Lst);
        Assert.AreEqual("List201", m201.Lst![0].Name);
    }

    /// <summary>
    /// 批量 Update json 列(整列赋值)：一次更新多行, 生成 col = CASE WHEN 主键 = @p THEN @val ... END,
    /// 断言多行 json 均按各自实体更新, 且互不串值。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_BatchUpdate_WholeColumns()
    {
        await Db.Insert([CreateJsonExecModel(210), CreateJsonExecModel(211)])
            .TagWith("批量json更新-准备数据")
            .ExecuteAsync(TestContext.CancellationToken);

        var e210 = CreateJsonExecModel(210);
        e210.Data = new JsonData() { Name = "Batch210", Value = 2100 };
        e210.Obj = new JsonObject { ["Name"] = "Node210", ["Age"] = 20 };
        var e211 = CreateJsonExecModel(211);
        e211.Data = new JsonData() { Name = "Batch211", Value = 2110 };
        e211.Obj = new JsonObject { ["Name"] = "Node211", ["Age"] = 21 };

        var effect = await Db.Update([e210, e211])
            .TagWith("批量json更新")
            .ExecuteAsync(TestContext.CancellationToken);
        Assert.AreEqual(2, effect);

        var list = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 210 || j.Id == 211)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, list);

        var m210 = list.Single(j => j.Id == 210);
        Assert.IsNotNull(m210.Data);
        Assert.AreEqual("Batch210", m210.Data!.Name);
        Assert.AreEqual(2100, m210.Data.Value);
        Assert.AreEqual("Node210", m210.Obj["Name"]!.GetValue<string>());
        Assert.AreEqual(20, m210.Obj["Age"]!.GetValue<int>());
        // 未显式改动的 json 列(Arr/Lst)仍应按实体整列写入
        Assert.IsNotNull(m210.Arr);
        Assert.AreEqual("Second210", m210.Arr![1].Name);
        Assert.IsNotNull(m210.Lst);
        Assert.AreEqual("List210", m210.Lst![0].Name);

        var m211 = list.Single(j => j.Id == 211);
        Assert.IsNotNull(m211.Data);
        Assert.AreEqual("Batch211", m211.Data!.Name);
        Assert.AreEqual(2110, m211.Data.Value);
        Assert.AreEqual("Node211", m211.Obj["Name"]!.GetValue<string>());
        Assert.AreEqual(21, m211.Obj["Age"]!.GetValue<int>());
    }

    /// <summary>
    /// 批量 Insert-Or-Update(upsert) json 列：同主键二次写入触发 UPDATE 分支
    /// (方言生成 DO UPDATE SET col = EXCLUDED.col), 断言 json 列被覆盖为新值。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_BatchInsertOrUpdate_WholeColumns()
    {
        await Db.Insert([CreateJsonExecModel(220), CreateJsonExecModel(221)])
            .TagWith("批量json upsert-准备数据")
            .ExecuteAsync(TestContext.CancellationToken);

        var e220 = CreateJsonExecModel(220);
        e220.Data = new JsonData() { Name = "Upsert220", Value = 2200 };
        e220.Obj = new JsonObject { ["Name"] = "UpsertNode220", ["Age"] = 30 };
        var e221 = CreateJsonExecModel(221);
        e221.Data = new JsonData() { Name = "Upsert221", Value = 2210 };
        e221.Obj = new JsonObject { ["Name"] = "UpsertNode221", ["Age"] = 31 };

        var effect = await Db.Insert([e220, e221])
            .OrUpdate()
            .TagWith("批量json upsert")
            .ExecuteAsync(TestContext.CancellationToken);
        // MySQL 的 ON DUPLICATE KEY UPDATE 命中已有行时受影响行数按 2 计(插入 1/更新 2),
        // 与 ExecutionTest 中其它 upsert 用例保持一致的分支断言。
        if (DbType == DbBaseType.MySql)
            Assert.AreEqual(4, effect);
        else
            Assert.AreEqual(2, effect);

        var list = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 220 || j.Id == 221)
            .ToListAsync(TestContext.CancellationToken);
        Assert.HasCount(2, list);
        Assert.AreEqual("Upsert220", list.Single(j => j.Id == 220).Data!.Name);
        Assert.AreEqual("UpsertNode220", list.Single(j => j.Id == 220).Obj["Name"]!.GetValue<string>());
        Assert.AreEqual("Upsert221", list.Single(j => j.Id == 221).Data!.Name);
        Assert.AreEqual("UpsertNode221", list.Single(j => j.Id == 221).Obj["Name"]!.GetValue<string>());
    }

    /// <summary>
    /// 读取路径的<b>复合末级成员</b>：投影嵌套对象(<c>Data.NestJson</c>)与数组/List 元素(<c>Arr[1]</c> / <c>Lst[0]</c>)。
    /// 本组用例此前所有 json 成员引用的末级都是标量(Name/Value)，从不把对象/数组<b>本身</b>作为末级取，
    /// 因此无法暴露"方言按 JSON 路径取非标量"这一类问题：Oracle / SqlServer 的读取分支都用裸
    /// <c>JSON_VALUE</c>(只返回标量，遇对象/数组得 NULL)，PG 则是末级 <c>->></c> + cast。
    /// </summary>
    [TestMethod]
    public async Task JsonColumn_CompositeMember_Projection()
    {
        // 末级为嵌套对象
        var nests = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToListAsync(j => new { j.Id, N = j.Data!.NestJson }, TestContext.CancellationToken);
        Assert.HasCount(1, nests);
        Assert.IsNotNull(nests[0].N);
        Assert.AreEqual("Nest Object100", nests[0].N!.Name);
        Assert.AreEqual(150, nests[0].N!.Value);

        var nests2 = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToListAsync(j =>  j.Data!.NestJson , TestContext.CancellationToken);
        Assert.HasCount(1, nests2);
        Assert.IsNotNull(nests2[0]);
        Assert.AreEqual("Nest Object100", nests2[0]!.Name);
        Assert.AreEqual(150, nests2[0]!.Value);

        // 末级为数组元素(对象)
        var items = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToListAsync(j => new { j.Id, I = j.Arr![1] }, TestContext.CancellationToken);
        Assert.HasCount(1, items);
        Assert.IsNotNull(items[0].I);
        Assert.AreEqual("Second100", items[0].I!.Name);
        Assert.AreEqual(160, items[0].I.Value);

        // 末级为 List 元素(对象)
        var lst = await Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToListAsync(j => new { j.Id, L = j.Lst![0] }, TestContext.CancellationToken);
        Assert.HasCount(1, lst);
        Assert.IsNotNull(lst[0].L);
        Assert.AreEqual("List100", lst[0].L!.Name);
        Assert.AreEqual(170, lst[0].L.Value);
    }
}
