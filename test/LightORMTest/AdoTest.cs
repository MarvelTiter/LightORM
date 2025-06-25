using System.Threading.Tasks;

namespace LightORMTest;

[TestClass]
public class AdoTest : TestBase
{
    [TestMethod]
    public async Task MultiUpdate()
    {
        await Parallel.ForAsync(0, 20, Update);
    }

    private async ValueTask Update(int i, CancellationToken cancellationToken)
    {
        await Task.Delay(i * 500, cancellationToken);
        using var scope = Db.CreateScoped();
        await scope.BeginAllTransactionAsync();
        var delay = Random.Shared.Next(100, 1500);
        await Task.Delay(delay, cancellationToken);
        var r = await scope.SwitchDatabase("v").Select<UserRole>().ToListAsync(cancellationToken);
        Console.WriteLine($"任务{i} -> 结果:{r.Count}");
        await scope.CommitAllTransactionAsync();
    }
}
