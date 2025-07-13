using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ResultTest;

public class Select : TestBase
{
    [TestMethod]
    public async Task Select_ToListAsync()
    {
        var list = await Db.Select<User>().ToListAsync();
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} - {item.UserName}");
        }
    }

    [TestMethod]
    public async Task Select_ToListAsync_With_Count()
    {
        var list = await Db.Select<User>()
            .Count(out var total)
            .ToListAsync();
        Console.WriteLine($"Total: {total}");
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} - {item.UserName}");
        }
    }

    [TestMethod]
    public async Task Select_ToEnumerableAsync()
    {
        var list = Db.Select<User>().ToEnumerableAsync();
        await foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} - {item.UserName}");
        }
    }

    [TestMethod]
    public void Select_Include_IncludeWhere_Result()
    {
        var result = Db.Select<User>()
            .Include(u => u.UserRoles)
            .Where(u => u.UserId == "admin")
            .ToList().FirstOrDefault();
        Assert.IsFalse(result is null);
        Assert.IsFalse(result.UserRoles is null);
        Assert.IsTrue(result.UserRoles.Count() == 2);

        result = Db.Select<User>()
            .Include(u => u.UserRoles.Where(r => r.RoleId.StartsWith("Ad")))
            .Where(u => u.UserId == "admin")
            .ToList().FirstOrDefault();

        Assert.IsFalse(result is null);
        Assert.IsFalse(result.UserRoles is null);
        Assert.IsTrue(result.UserRoles.Count() == 1);
        Assert.IsTrue(result.UserRoles.FirstOrDefault()!.RoleId == "Admin");

        result = Db.Select<User>()
            .Include(u => u.UserRoles.Where(r => r.RoleId.StartsWith("Su")))
            .Where(u => u.UserId == "admin")
            .ToList().FirstOrDefault();
        Assert.IsFalse(result is null);
        Assert.IsFalse(result.UserRoles is null);
        Assert.IsTrue(result.UserRoles.Count() == 1);
        Assert.IsTrue(result.UserRoles.FirstOrDefault()!.RoleId == "SuperAdmin");
    }
}
