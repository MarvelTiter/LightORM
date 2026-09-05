using LightORM.Extension;
using LightORM.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace LightORMTest.ResultTest;

/// <summary>
/// 写操作细节 / ISqlExecutor 原生执行补缺 / ILightOrmRepository 异步族接口方法覆盖用例（基类）
/// 覆盖：Insert(无实体, Set 逐列) / Insert ToSql/ToSqlWithParameters/Execute /
///      Update.UpdateColumns{TUpdate} / Update.WhereIf / Delete.FullDelete() 无参 /
///      Ado ExecuteReader / QueryAsync / ExecuteNonQueryAsync / ExecuteResult.Single(Async) /
///      QueryMultipleAsync + ReadFirst(Async) / 仓储 Table / ExpSelect / Delete(key) / 异步族
/// </summary>
public partial class ExecutionTest
{
    [TestMethod]
    public async Task Insert_NoEntity_Test()
    {
        // 无实体插入：通过 Set 逐列指定值
        await Db.Insert<User>()
            .Set(u => u.UserId, "noe01")
            .Set(u => u.UserName, "Noe")
            .Set(u => u.Password, "p")
            .Set(u => u.Sign, SignType.Vip)
            .Set(u => u.IsLock, false)
            .Set(u => u.Version, 1)
            .ExecuteAsync(TestContext.CancellationToken);
        var u = await Db.Select<User>().Where(x => x.UserId == "noe01").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("Noe", u.UserName);
        Assert.AreEqual(SignType.Vip, u.Sign);
        Assert.IsFalse(u.IsLock);
    }

    [TestMethod]
    public async Task Insert_ToSql_Execute_Test()
    {
        // ToSql / ToSqlWithParameters 仅生成 SQL，不执行
        var sql = Db.Insert(new User { UserId = "sy01", UserName = "Sy1", Password = "p", Sign = SignType.None, IsLock = false, Version = 1 }).ToSql();
        StringAssert.StartsWith(sql, "INSERT INTO");

        var sqlWithParams = Db.Insert(new User { UserId = "sy01", UserName = "Sy1", Password = "p", Sign = SignType.None, IsLock = false, Version = 1 }).ToSqlWithParameters();
        StringAssert.StartsWith(sqlWithParams, "INSERT INTO");

        // 同步 Execute
        var e = Db.Insert(new User { UserId = "sy01", UserName = "Sy1", Password = "p", Sign = SignType.None, IsLock = false, Version = 1 }).Execute();
        Assert.AreEqual(1, e);
        var u = await Db.Select<User>().Where(x => x.UserId == "sy01").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(u);
        Assert.AreEqual("Sy1", u.UserName);
    }

    [TestMethod]
    public async Task Update_Columns_Test()
    {
        // UpdateColumns<TUpdate>(exp) 只更新指定列
        var u = await Db.Select<User>().Where(x => x.UserId == "test01").FirstAsync(TestContext.CancellationToken);
        u!.UserName = "ColsUpdated";
        u.Age = 99;
        u.Sign = SignType.Vip; // 不在更新列中，应保持不变
        await Db.Update(u)
            .UpdateColumns(x => new { x.UserName, x.Age })
            .ExecuteAsync(TestContext.CancellationToken);

        var reloaded = await Db.Select<User>().Where(x => x.UserId == "test01").FirstAsync(TestContext.CancellationToken);
        Assert.IsNotNull(reloaded);
        Assert.AreEqual("ColsUpdated", reloaded.UserName);
        Assert.AreEqual(99, reloaded.Age);
        Assert.AreEqual(SignType.None, reloaded.Sign); // 未更新，保持初始值
    }

    [TestMethod]
    public async Task Update_WhereIf_Test()
    {
        // Update 链路的 WhereIf
        await Db.Update<User>()
            .Set(u => u.UserName == "WhereIf")
            .WhereIf(true, u => u.UserId == "test01")
            .WhereIf(false, u => u.UserId == "test02")
            .ExecuteAsync(TestContext.CancellationToken);
        var u1 = await Db.Select<User>().Where(x => x.UserId == "test01").FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual("WhereIf", u1?.UserName);
        var u2 = await Db.Select<User>().Where(x => x.UserId == "test02").FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual("Test2", u2?.UserName);
    }

    [TestMethod]
    public async Task Delete_FullDelete_NoTruncate_Test()
    {
        // FullDelete() 无参重载：DELETE 清空表
        await Db.Delete<Sales>().FullDelete().ExecuteAsync(TestContext.CancellationToken);
        var count = await Db.Select<Sales>().CountAsync(TestContext.CancellationToken);
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task Ado_ExecuteReader_Test()
    {
        var prefix = CurrentDefaultProvider.DatabaseAdapter.Prefix;
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");
        using var reader = Db.Ado.ExecuteReader($"select * from {userTable} where age > {prefix}Age", new { Age = 10 });
        int count = 0;
        while (reader.Read())
        {
            count++;
        }
        Assert.AreEqual(4, count);
    }

    [TestMethod]
    public async Task Ado_QueryAsync_Test()
    {
        var prefix = CurrentDefaultProvider.DatabaseAdapter.Prefix;
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");
        int count = 0;
        await foreach (var u in Db.Ado.QueryAsync<User>($"select * from {userTable} where age > {prefix}Age", new { Age = 10 }, cancellationToken: TestContext.CancellationToken))
        {
            Assert.IsNotNull(u.UserId);
            count++;
        }
        Assert.AreEqual(4, count);
    }

    [TestMethod]
    public async Task Ado_ExecuteNonQueryAsync_Test()
    {
        var prefix = CurrentDefaultProvider.DatabaseAdapter.Prefix;
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");
        var nonQuery = await Db.Ado.ExecuteNonQueryAsync($"""
            update {userTable} set user_name = {prefix}Name where user_id = {prefix}UserId
            """, new { Name = "AdoUpdAsync", UserId = "test01" }, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(1, nonQuery);
        var u = await Db.Select<User>().Where(x => x.UserId == "test01").FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual("AdoUpdAsync", u?.UserName);
    }

    [TestMethod]
    public async Task Ado_ExecuteResult_Single_Test()
    {
        var prefix = CurrentDefaultProvider.DatabaseAdapter.Prefix;
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");
        var single = Db.Ado.Execute($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test01" }).Single<User>();
        Assert.AreEqual("test01", single?.UserId);
        var singleAsync = await Db.Ado.Execute($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test02" }).SingleAsync<User>(TestContext.CancellationToken);
        Assert.AreEqual("test02", singleAsync?.UserId);
    }

    [TestMethod]
    public async Task Ado_QueryMultipleAsync_Test()
    {
        // 多结果集异步 + ReadFirst / ReadFirstAsync（注意：部分方言需开启多语句支持）
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");
        var roleTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("ROLE");
        using var multi = await Db.Ado.QueryMultipleAsync([$"select * from {userTable}", $" select * from {roleTable}"], cancellationToken: TestContext.CancellationToken);
        var firstUser = multi.ReadFirst<User>();
        var firstRole = await multi.ReadFirstAsync<Role>();
        Assert.IsNotNull(firstUser);
        Assert.IsNotNull(firstRole);
    }

    [TestMethod]
    public async Task Repository_Async_Operations_Test()
    {
        using var repo = Services.GetRequiredService<ILightOrmRepository<User>>();

        // ExpSelect 属性
        var viaExpSelect = repo.ExpSelect.Where(u => u.UserId == "test01").First();
        Assert.IsNotNull(viaExpSelect);

        // Table 属性（IQueryable）
        var viaTable = repo.Table.Where(u => u.Age > 10).ToList();
        Assert.HasCount(4, viaTable);

        // InsertRangeAsync + SaveChanges
        var inserted = await repo.InsertRangeAsync([
            new User { UserId = "repo11", UserName = "Repo11", Password = "p", IsLock = false, Sign = SignType.None,LastLogin = DateTime.Now },
            new User { UserId = "repo12", UserName = "Repo12", Password = "p", IsLock = false, Sign = SignType.None }], TestContext.CancellationToken);
        Assert.AreEqual(2, inserted);
        Assert.IsTrue(repo.SaveChanges());

        // UpdateAsync + SaveChanges
        var toUpdate = await Db.Select<User>().Where(x => x.UserId == "repo11").FirstAsync(TestContext.CancellationToken);
        toUpdate!.UserName = "Repo11_Updated";
        Assert.AreEqual(1, await repo.UpdateAsync(toUpdate, TestContext.CancellationToken));
        Assert.IsTrue(repo.SaveChanges());

        // UpdateRange / UpdateRangeAsync
        var range = await Db.Select<User>().Where(x => x.UserId == "repo11" || x.UserId == "repo12").ToListAsync(TestContext.CancellationToken);
        foreach (var u in range) u.Age = 55;
        Assert.AreEqual(2, repo.UpdateRange(range));
        Assert.IsTrue(repo.SaveChanges());
        range = await Db.Select<User>().Where(x => x.UserId == "repo11" || x.UserId == "repo12").ToListAsync(TestContext.CancellationToken);
        foreach (var u in range) u.Age = 66;
        Assert.AreEqual(2, await repo.UpdateRangeAsync(range, TestContext.CancellationToken));
        Assert.IsTrue(repo.SaveChanges());

        // DeleteRangeAsync + Delete(主键) + SaveChanges
        var toDelete = await Db.Select<User>().Where(x => x.UserId == "repo11" || x.UserId == "repo12").ToListAsync(TestContext.CancellationToken);
        Assert.AreEqual(2, await repo.DeleteRangeAsync(toDelete, TestContext.CancellationToken));
        Assert.IsTrue(repo.SaveChanges());
        Assert.AreEqual(1, repo.Delete("test01"));
        Assert.IsTrue(repo.SaveChanges());
        var deleted = await Db.Select<User>().Where(x => x.UserId == "repo11" || x.UserId == "repo12" || x.UserId == "test01").CountAsync(TestContext.CancellationToken);
        Assert.AreEqual(0, deleted);

        // DeleteFullAsync
        Assert.IsGreaterThanOrEqualTo(-1, await repo.DeleteFullAsync(truncate: true, cancellationToken: TestContext.CancellationToken));
        Assert.IsTrue(repo.SaveChanges());
        var remain = await Db.Select<User>().CountAsync(TestContext.CancellationToken);
        Assert.AreEqual(0, remain);
    }
}
