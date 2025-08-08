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

    [TestMethod]
    public void AutoPrimaryKeyTest()
    {
        var usrRepo = Services.GetRequiredService<ILightOrmRepository<User>>();
        usrRepo.DeleteFull(true);
        usrRepo.InsertRange([new User()
        {
            UserId = "test01",
            UserName = "Test1",
            Age = 18,
            IsLock = false,
            Password = "helloworld",
        }, new User()
        {
            UserId = "test02",
            UserName = "Test2",
            Age = 18,
            IsLock = true,
            Password = "helloworld",
        }]);
        var list = usrRepo.Table.ToList();
        var user1 = list[0];
        var user2 = list[1];
        var u1 = usrRepo.GetOneByKey(user1.Id);
        Assert.IsTrue(u1?.UserId == user1.UserId);
        var u2 = usrRepo.GetOneByKey(user2.UserId);
        Assert.IsTrue(u2?.Id == user2.Id);
    }
}
