using System.Threading.Tasks;

namespace LightORMTest;

public class AdoTest : TestBase
{
    [TestMethod]
    public async Task TestTransactionContext()
    {
        await Parallel.ForAsync(0, 20, Update);
    }

    private async ValueTask Update(int i, CancellationToken cancellationToken)
    {
        using var scope = Db.CreateScoped();
        await scope.BeginAllTransactionAsync();
        var delay = Random.Shared.Next(10, 100);
        await Task.Delay(delay, cancellationToken);
        var r = await scope.Select<UserRole>().ToListAsync(cancellationToken);
        Console.WriteLine($"任务{i} -> 结果:{r.Count}");
        await scope.CommitAllTransactionAsync();
    }
}
