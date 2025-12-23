using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest;

public class BulkCopyTest : TestBase
{
    public virtual IEnumerable<User> GetTestUsers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var u = new User
            {
                Id = i,
                UserId = $"{i}",
                UserName = string.Concat("User_", Guid.NewGuid().ToString("N").AsSpan(0, 8)),
                Password = "123456",
                Age = new Random().Next(18, 60),
                ModifyTime = DateTime.Now, 
            };
            u.Sign = u.Age > 50 ? SignType.Svip : SignType.Vip;
            u.IsLock = u.Age > 30;
            yield return u;
        }
    }
    [TestMethod]
    public void BulkCopy()
    {
        Db.Delete<User>().FullDelete(true).Execute();
        Db.Ado.BulkCopy(GetTestUsers(20));
    }
}
