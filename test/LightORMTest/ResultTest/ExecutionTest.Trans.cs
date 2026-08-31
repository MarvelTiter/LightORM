using System.Collections.Concurrent;
using System.Data;

namespace LightORMTest.ResultTest;

public partial class ExecutionTest
{
    [TestMethod]
    public async Task Transaction_BasicCommit_Test()
    {
        // Arrange
        var userId = "test01";
        var originalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
        var originalAge = originalUser?.Age;

        using var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        try
        {
            // Act
            await scope.Update<User>()
                .Set(u => u.Age, originalAge + 10)
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            await scope.CommitTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // Assert - 验证提交成功
            var updatedUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual(originalAge + 10, updatedUser?.Age, "事务提交后数据应该被更新");
        }
        catch
        {
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);
            throw;
        }
    }

    [TestMethod]
    public async Task Transaction_Rollback_Test()
    {
        // Arrange
        var userId = "test01";
        var originalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
        var originalAge = originalUser?.Age;

        using var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        try
        {
            // Act - 执行更新
            await scope.Update<User>()
                .Set(u => u.Age, originalAge + 999)
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            // 回滚事务
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // Assert - 验证回滚成功
            var rolledBackUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual(originalAge, rolledBackUser?.Age, "事务回滚后数据应该恢复原值");
        }
        catch
        {
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);
            throw;
        }
    }

    [TestMethod]
    public async Task Transaction_Atomicity_Test()
    {
        // Arrange
        var userId = "test01";
        var originalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
        var originalAge = originalUser!.Age;
        using var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        // Act & Assert
        try
        {
            // 操作1: 更新用户
            await scope.Update<User>()
                .Set(u => u.Age, originalAge + 100)
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            // 操作2: 插入无效数据（故意触发异常）
            await scope.Insert(new User()
            {
                UserId = "invalid!@#",
                UserName = new string('c', 500),// 超出最大长度限制
                Age = 999
            }).ExecuteAsync(TestContext.CancellationToken);

            // 如果执行到这里，说明插入应该失败，但失败了就不会走到这里
            await scope.CommitTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // 如果事务提交，验证数据
            var finalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual(originalAge + 100, finalUser?.Age);
        }
        catch (Exception ex)
        {
            // 回滚事务
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // 验证数据没有被修改
            var rolledBackUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual(originalAge, rolledBackUser?.Age, "事务异常回滚后数据应该恢复");

            Console.WriteLine($"事务回滚成功: {ex.Message}");
            return;
        }

        Assert.Fail("应该抛出异常并回滚事务");
    }

    [TestMethod]
    public async Task Transaction_MultiTable_Test()
    {
        // Arrange
        var userId = "test01";
        var roleId = "Admin";

        using var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        try
        {
            // Act - 同时操作多个表
            // 1. 更新用户信息
            await scope.Update<User>()
                .Set(u => u.UserName, "UpdatedName")
                .Set(u => u.Age, 99)
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            // 2. 添加新的用户角色
            await scope.Insert(new UserRole()
            {
                UserId = userId,
                RoleId = "SuperAdmin"
            }).ExecuteAsync(TestContext.CancellationToken);

            // 3. 更新角色名称
            await scope.Update<Role>()
                .Set(r => r.RoleName, "超级管理员(已更新)")
                .Where(r => r.RoleId == roleId)
                .ExecuteAsync(TestContext.CancellationToken);

            await scope.CommitTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // Assert - 验证所有操作都成功
            var user = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual("UpdatedName", user?.UserName);
            Assert.AreEqual(99, user?.Age);

            var roles = await Db.Select<UserRole>().Where(ur => ur.UserId == userId).ToListAsync(TestContext.CancellationToken);
            Assert.IsTrue(roles.Any(r => r.RoleId == "SuperAdmin"), "SuperAdmin 角色应该被添加");

            var role = await Db.Select<Role>().Where(r => r.RoleId == roleId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual("超级管理员(已更新)", role?.RoleName);
        }
        catch
        {
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);
            throw;
        }
    }

    [TestMethod]
    public async Task Transaction_Nested_Test()
    {
        // Arrange
        var userId = "test01";

        using var scope = Db.CreateScoped();

        // 外层事务
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        try
        {
            // 外层操作
            await scope.Update<User>()
                .Set(u => u.UserName, "OuterUpdate")
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            // 嵌套事务 - 开启 Savepoint
            await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken); // 嵌套开启

            try
            {
                // 内层操作
                await scope.Update<User>()
                    .Set(u => u.Age, 888)
                    .Where(u => u.UserId == userId)
                    .ExecuteAsync(TestContext.CancellationToken);

                // 内层提交（实际只是减少嵌套计数）
                await scope.CommitTransactionAsync(cancellationToken: TestContext.CancellationToken);
            }
            catch
            {
                // 内层回滚（回滚到 Savepoint）
                await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);
                throw;
            }

            // 外层提交（真实提交）
            await scope.CommitTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // Assert - 验证所有更改生效
            var user = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual("OuterUpdate", user?.UserName);
            Assert.AreEqual(888, user?.Age);
        }
        catch
        {
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);
            throw;
        }
    }

    [TestMethod]
    public async Task Transaction_Concurrency_Test()
    {
        // Arrange
        var userId = "test01";
        var initialUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);

        // 使用信号量控制并发
        var semaphore = new SemaphoreSlim(5); // 限制并发数
        var errors = new ConcurrentBag<Exception>();
        var successCount = 0;
        var conflictCount = 0;

        await Parallel.ForAsync(0, 20, async (i, ct) =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                using var scope = Db.CreateScoped();
                await scope.BeginTransactionAsync(IsolationLevel.ReadCommitted, TestContext.CancellationToken);

                // 读取当前值
                var currentUser = await scope.Select<User>()
                    .Where(u => u.UserId == userId)
                    .FirstAsync(ct);

                // 模拟业务延迟增加并发冲突
                await Task.Delay(Random.Shared.Next(10, 50), ct);

                // 使用乐观锁更新
                var newAge = currentUser?.Age + 1;
                var affected = await scope.Update<User>()
                    .Set(u => u.Age, newAge)
                    .WithVersion(u => u.Version, currentUser?.Version)
                    .Where(u => u.UserId == userId) // 乐观锁
                    .ExecuteAsync(ct);

                if (affected > 0)
                {
                    Interlocked.Increment(ref successCount);
                    Console.WriteLine($"任务{i}: 更新成功, Age = {newAge}");
                }
                else
                {
                    Interlocked.Increment(ref conflictCount);
                    Console.WriteLine($"任务{i}: 并发冲突, 版本号不匹配");
                }

                await scope.CommitTransactionAsync("MainDb", TestContext.CancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                Console.WriteLine($"任务{i} 异常: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        });

        // Assert
        Assert.IsEmpty(errors, $"发生异常: {string.Join(Environment.NewLine, errors.Select(e => e.Message))}");
        Assert.IsGreaterThan(0, successCount, "至少应该有一次成功更新");
        Assert.IsGreaterThan(0, conflictCount, "应该检测到并发冲突");

        // 验证最终数据一致性
        var finalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
        var expectedAge = initialUser?.Age + successCount;
        Assert.AreEqual(expectedAge, finalUser?.Age, $"最终年龄应为 {initialUser?.Age} + {successCount} = {expectedAge}");
    }

    [TestMethod]
    public async Task Transaction_WithInclude_Test()
    {
        // Arrange
        var userId = "test01";

        using var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        try
        {
            // Act - 在事务中查询 Include 数据
            var user = await scope.Select<User>()
                .Include(u => u.UserRoles)
                .Where(u => u.UserId == userId)
                .FirstAsync(TestContext.CancellationToken);

            // 在事务中更新
            await scope.Update<User>()
                .Set(u => u.UserName, "TransactionWithInclude")
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            // 验证 Include 数据正确
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.UserRoles);
            Assert.IsTrue(user.UserRoles.Any());

            foreach (var ur in user.UserRoles)
            {
                Assert.IsNotNull(ur);
                Console.WriteLine($"User: {user.UserName}, Role: {ur.RoleName}");
            }

            await scope.CommitTransactionAsync(cancellationToken: TestContext.CancellationToken);

            // Assert - 验证更新生效
            var updatedUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
            Assert.AreEqual("TransactionWithInclude", updatedUser?.UserName);
        }
        catch
        {
            await scope.RollbackTransactionAsync(cancellationToken: TestContext.CancellationToken);
            throw;
        }
    }

    [TestMethod]
    public async Task Transaction_CleanupOnException_Test()
    {
        // Arrange
        var userId = "test01";
        var originalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
        var originalAge = originalUser?.Age;

        // Act - 不使用 using，手动管理
        var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        try
        {
            // 执行更新
            await scope.Update<User>()
                .Set(u => u.Age, originalAge + 1000)
                .Where(u => u.UserId == userId)
                .ExecuteAsync(TestContext.CancellationToken);

            // 故意抛出异常
            throw new InvalidOperationException("模拟业务异常");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"捕获异常: {ex.Message}");
            // 注意：这里没有调用 Rollback 或 Commit
            // 依赖 scope 的 Dispose 自动处理
        }
        finally
        {
            // 手动释放（会触发自动回滚）
            scope.Dispose();
        }

        // Assert - 验证数据没有被修改
        var finalUser = await Db.Select<User>().Where(u => u.UserId == userId).FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual(originalAge, finalUser?.Age, "异常发生后数据应该保持不变");
    }

    [TestMethod]
    public async Task Transaction_Cancellation_Test()
    {
        var userId = "test01";

        using var scope = Db.CreateScoped();
        await scope.BeginTransactionAsync(cancellationToken: TestContext.CancellationToken);

        // 使用 CancellationTokenSource 模拟超时
        var timeout = Random.Shared.Next(1, 20);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout));

        try
        {
            // 执行一个长时间操作
            await scope.Update<User>()
                .Set(u => u.UserName, "CancellationTest")
                .Where(u => u.UserId == userId)
                .ExecuteAsync(cts.Token);
            await scope.CommitTransactionAsync(cts.Token);
            Console.WriteLine("操作完成");
        }
        catch (OperationCanceledException)
        {
            // 操作被取消
            await scope.RollbackTransactionAsync(cts.Token);
            Console.WriteLine("操作被取消，事务已回滚");
            return;
        }
    }
}
