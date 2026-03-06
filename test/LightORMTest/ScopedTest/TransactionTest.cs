using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LightORMTest.ScopedTest;

public class TransactionTest : TestBase
{
    [TestMethod]
    public async Task MultiCommit()
    {
        await SingleUpdateAsync("ROOT", "User", "Setting");
        await SingleUpdateAsync("ROOT", "User", "Setting", "Permission");
    }

    [TestMethod]
    public async Task ScopedCommit()
    {
        await UpdateAsync();
        await UpdateAsync();
    }

    private async Task SingleUpdateAsync(params string[] pp)
    {
        using var scoped = Db.CreateScoped("MainDb");
        var role = new Role()
        {
            RoleId = "TT001",
            RoleName = "测试",
            Powers = pp.Select(p => new Permission() { PermissionId = p })
        };
        try
        {
            await scoped.BeginTransactionAsync();
            var n = await scoped.Update(role)
                .Where(r => r.RoleId == role.RoleId)
                .ExecuteAsync();
            var roleId = role.RoleId;
            string[] powers = [.. role.Powers.Select(p => p.PermissionId).Distinct()];
            var d = await scoped.Delete<RolePermission>().Where(r => r.RoleId == roleId).ExecuteAsync();
            var i = 0;

            var ef = await scoped.Insert<RolePermission>([..powers.Select(p => new RolePermission()
            {
                RoleId = roleId,
                PermissionId = p
            })]).ExecuteAsync();
            i += ef;
            if (powers.Length == 3)
            {
                await scoped.CommitTransactionAsync();
            }
            else
            {
                await scoped.RollbackTransactionAsync();
            }
        }
        catch
        {
            await scoped.RollbackTransactionAsync();
        }
    }

    private async Task UpdateAsync()
    {
        using var scoped = Db.CreateScoped();
        await scoped.BeginTransactionAsync("v");
        await scoped.Select<UserRole>().ToListAsync();
        await scoped.SwitchDatabase("v").Select<UserRole>().ToListAsync();
        await scoped.CommitTransactionAsync("v");
    }

    [TestMethod]
    public async Task TestTransactionScope()
    {
        //await Db.Insert(new User()
        //{
        //    Id = 1,
        //    Age = 20,
        //    IsLock = true,
        //    Password = "password",
        //    Sign = SignType.Vip,
        //    UserName = "username",
        //    UserId = "admin",
        //}).ExecuteAsync();

        using var tc = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var user = (await Db.Select<User>().FirstAsync()) ?? throw new Exception("没有查询到结果");

        await Db.Update<User>(user)
            .Set(u => u.Age, 18)
            .Where(u => u.UserId == user.UserId && u.Version == user.Version)
            .ExecuteAsync();
        tc.Complete();
    }
}
