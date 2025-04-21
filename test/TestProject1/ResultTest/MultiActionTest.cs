namespace TestProject1.ResultTest
{
    [TestClass]
    public class MultiActionTest : TestBase
    {
        const string UID = "TEST_USER";
        [TestMethod]
        public void InsertEntity()
        {
            int hour = DateTime.Now.Hour;
            var u = Db.Select<User>().First()!;
            u.UserId = UID;
            u.Age = hour;
            Db.Insert(u).Execute();
            var iu = Db.Select<User>().Where(u => u.UserId == UID).First();
            Assert.IsTrue(iu?.Age == hour);
            Db.Delete<User>(iu).Execute();
            var iu2 = Db.Select<User>().Where(u => u.UserId == UID).First();
            Assert.IsTrue(iu2 is null);
        }
    }
}
