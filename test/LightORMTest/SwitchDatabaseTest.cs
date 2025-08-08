using LightORM.Providers.Sqlite.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest;

[TestClass]
public class SwitchDatabaseTest
{
    public IExpressionContext Db { get; }
    public SwitchDatabaseTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLightOrm(option =>
        {
            option.UseSqlite("db1", "");
            option.UseSqlite("db2", "");
            option.UseSqlite("db3", "");
            option.UseSqlite("db4", "");
            option.UseSqlite("db5", "");
            option.SetDefault("db1");
            option.UseInterceptor<LightOrmAop>();
        });

        var provider = services.BuildServiceProvider();

        Db = provider.GetRequiredService<IExpressionContext>();
    }

    [TestMethod]
    public void TestConfig()
    {
        var db1 = Db.Select<User>().ToSql();
        Console.WriteLine(db1);
    }

    [TestMethod]
    public async Task TestSwitchDatabase()
    {
        var random = new Random();
        await Parallel.ForAsync(0, 100, async (_, c) =>
          {
              for (int i = 0; i < 20; i++)
              {
                  var index = random.Next(1, 6);
                  var wait = random.Next(1, 50);
                  var dbKey = $"db{index}";
                  Assert.IsTrue(Db.SwitchDatabase(dbKey).Key == dbKey);
                  await Task.Delay(wait, c);
              }
          });
    }

}
