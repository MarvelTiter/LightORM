using LightORM.Extension;
using LightORM.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace LightORMTest.ResultTest;

/// <summary>
/// ISqlExecutor/SqlAdo 原生执行、ILightOrmRepository 仓储、Scoped 上下文接口方法覆盖用例（基类）
/// 覆盖：ExecuteNonQuery / ExecuteScalar(Async) / ExecuteDataTable / ExecuteDataSet /
///      Query / QuerySingle(Async) / ExecuteResult / QueryMultiple / 仓储全套 /
///      Scoped 事务提交与回滚
/// </summary>
public partial class ExecutionTest
{
    [TestMethod]
    public async Task Ado_Executor_Methods_Test()
    {
        var prefix = CurrentDefaultProvider.DatabaseAdapter.Prefix;
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");

        // ExecuteNonQuery
        var nonQuery = Db.Ado.ExecuteNonQuery($"""
            update {userTable} set user_name = {prefix}Name where user_id = {prefix}UserId
            """, new { Name = "AdoUpdate", UserId = "test01" });
        Assert.AreEqual(1, nonQuery);

        // ExecuteScalar / ExecuteScalarAsync
        int count = Db.Ado.ExecuteScalar($"select count(*) from {userTable}", new { });
        Assert.AreEqual(6, count);
        int count2 = await Db.Ado.ExecuteScalarAsync($"select count(*) from {userTable}", new { }, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(6, count2);

        // ExecuteDataTable
        var dt = Db.Ado.ExecuteDataTable($"select * from {userTable} where age > {prefix}Age", new { Age = 10 });
        Assert.AreEqual(4, dt.Rows.Count);

        // ExecuteDataSet
        var ds = Db.Ado.ExecuteDataSet($"select * from {userTable}", new { });
        Assert.AreEqual(1, ds.Tables.Count);
        Assert.AreEqual(6, ds.Tables[0].Rows.Count);

        // Query / QuerySingle / QuerySingleAsync
        var q = Db.Ado.Query<User>($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test01" });
        Assert.HasCount(1, q);
        var single = Db.Ado.QuerySingle<User>($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test02" });
        Assert.AreEqual("test02", single?.UserId);
        var singleAsync = await Db.Ado.QuerySingleAsync<User>($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test03" }, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual("test03", singleAsync?.UserId);

        // Execute -> ExecuteResult.ToList / SingleAsync
        var viaExecute = Db.Ado.Execute($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test04" }).ToList<User>();
        Assert.HasCount(1, viaExecute);
        var viaSingle = await Db.Ado.Execute($"select * from {userTable} where user_id = {prefix}Id", new { Id = "test05" }).SingleAsync<User>(TestContext.CancellationToken);
        Assert.AreEqual("test05", viaSingle?.UserId);
    }

    [TestMethod]
    public async Task Ado_QueryMultiple_Test()
    {
        // 一条命令多结果集（注意：部分方言需开启多语句支持）
        var userTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("USER");
        var roleTable = CurrentDefaultProvider.DatabaseAdapter.AttachEmphasis("ROLE");
        using var multi = Db.Ado.QueryMultiple([$"select * from {userTable}", $"select * from {roleTable}"]);
        var users = await multi.ReadListAsync<User>();
        var roles = await multi.ReadListAsync<Role>();
        Assert.HasCount(6, users);
        Assert.HasCount(3, roles);
    }

    [TestMethod]
    public async Task Repository_Methods_Test()
    {
        using var repo = Services.GetRequiredService<ILightOrmRepository<User>>();

        // GetOne / GetOneByKey（复合主键 UserId）
        var u1 = repo.GetOne(x => x.UserId == "test01");
        Assert.IsNotNull(u1);
        var u2 = repo.GetOneByKey("test01");
        Assert.IsNotNull(u2);
        Assert.AreEqual(u1!.Id, u2!.Id);

        // Insert + SaveChanges
        var inserted = repo.Insert(new User { UserId = "repo01", UserName = "Repo1", Password = "p", IsLock = false, Sign = SignType.None });
        Assert.IsGreaterThan(0, inserted);
        Assert.IsTrue(repo.SaveChanges());

        // InsertRange + SaveChanges
        var insertedRange = repo.InsertRange([
            new User { UserId = "repo02", UserName = "Repo2", Password = "p", IsLock = false, Sign = SignType.None },
            new User { UserId = "repo03", UserName = "Repo3", Password = "p", IsLock = false, Sign = SignType.None }]);
        Assert.AreEqual(2, insertedRange);
        Assert.IsTrue(repo.SaveChanges());

        // Update + SaveChanges
        var toUpdate = await Db.Select<User>().Where(x => x.UserId == "repo02").FirstAsync(TestContext.CancellationToken);
        toUpdate!.UserName = "Repo2_Updated";
        Assert.IsGreaterThan(0, repo.Update(toUpdate));
        Assert.IsTrue(repo.SaveChanges());
        var updated = await Db.Select<User>().Where(x => x.UserId == "repo02").FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual("Repo2_Updated", updated!.UserName);

        // Delete(entity) + DeleteRange + SaveChanges
        var toDelete = await Db.Select<User>().Where(x => x.UserId == "repo01").FirstAsync(TestContext.CancellationToken);
        var toDeleteRange = await Db.Select<User>().Where(x => x.UserId == "repo02" || x.UserId == "repo03").ToListAsync(TestContext.CancellationToken);
        Assert.AreEqual(1, repo.Delete(toDelete!));
        Assert.AreEqual(2, repo.DeleteRange(toDeleteRange));
        Assert.IsTrue(repo.SaveChanges());
        var remain = await Db.Select<User>().Where(x => x.UserId == "repo01" || x.UserId == "repo02" || x.UserId == "repo03").CountAsync(TestContext.CancellationToken);
        Assert.AreEqual(0, remain);

        // 异步 InsertAsync / DeleteAsync
        var insAsync = await repo.InsertAsync(new User { UserId = "repo04", UserName = "Repo4", Password = "p", IsLock = false, Sign = SignType.None }, TestContext.CancellationToken);
        Assert.IsGreaterThan(0, insAsync);
        Assert.AreEqual(1, await repo.DeleteAsync("repo04", cancellationToken: TestContext.CancellationToken));
        Assert.IsTrue(repo.SaveChanges());
    }

    [TestMethod]
    public async Task Scoped_Context_Test()
    {
        // Scoped 上下文：独立事务，提交与回滚
        using var scoped = Db.CreateScoped();
        await scoped.Delete<User>().Where(u => u.UserId == "scoped01").ExecuteAsync(TestContext.CancellationToken);
        await scoped.Insert(new User { UserId = "scoped01", UserName = "Scoped1", Password = "p", IsLock = false, Sign = SignType.None, Version = 1 })
            .ExecuteAsync(TestContext.CancellationToken);

        await scoped.BeginTransactionAsync();
        await scoped.Update<User>().Set(u => u.UserName == "ScopedTx").Where(u => u.UserId == "scoped01").ExecuteAsync(TestContext.CancellationToken);
        await scoped.CommitTransactionAsync();
        var u = await scoped.Select<User>().Where(x => x.UserId == "scoped01").FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual("ScopedTx", u?.UserName);

        // 回滚路径：修改后回滚，值保持 "ScopedTx"
        await scoped.BeginTransactionAsync();
        await scoped.Update<User>().Set(u => u.UserName == "ScopedRollback").Where(u => u.UserId == "scoped01").ExecuteAsync(TestContext.CancellationToken);
        await scoped.RollbackTransactionAsync();
        u = await scoped.Select<User>().Where(x => x.UserId == "scoped01").FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual("ScopedTx", u?.UserName);
    }
}
