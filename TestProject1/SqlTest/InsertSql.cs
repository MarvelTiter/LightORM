using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.SqlTest
{
    [TestClass]
    public class InsertSql : TestBase
    {
        const string UID = "TEST_USER";
        [TestMethod]
        public void InsertEntity()
        {
            var u = Db.Select<User>().First()!;
            u.UserId = UID;
            u.Age = DateTime.Now.Hour;
            Db.Insert(u).Execute();
            var iu = Db.Select<User>().Where(u => u.UserId == UID).First();
            Assert.IsTrue(iu?.Age == DateTime.Now.Hour);
            Db.Delete<User>(iu).Execute();
            var iu2 = Db.Select<User>().Where(u => u.UserId == UID).First();
            Assert.IsTrue(iu2 is null);
        }
    }
}
