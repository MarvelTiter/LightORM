using LightORM.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest;


public class RepositoryTest : TestBase
{
    [TestMethod]
    public void TestRepositoryInsert()
    {
        var usrRepo = Services.GetRequiredService<ILightOrmRepository<User>>();
        usrRepo.Insert(new User()
        {
            UserId = "test01",
            UserName = "Test",
            Age = 18,
            IsLock = false,
            Password = "helloworld",
        });

        var iu = usrRepo.Table.Where(u => u.UserId == "test01").FirstOrDefault();
        Assert.IsTrue(iu?.UserName == "Test");
        usrRepo.DeleteFull(truncate: true);
    }
}
