using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest.ResultTest;

[TestClass]
public class Select : TestBase
{
    [TestMethod]
    public async Task S1_Select_ToListAsync()
    {
        var list = await Db.Select<User>().ToListAsync();
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} - {item.UserName}");
        }
    }

    [TestMethod]
    public async Task S1_Select_ToListAsync_With_Count()
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
    public async Task S1_Select_ToEnumerableAsync()
    {
        var list = Db.Select<User>().ToEnumerableAsync();
        await foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} - {item.UserName}");
        }
    }
}
