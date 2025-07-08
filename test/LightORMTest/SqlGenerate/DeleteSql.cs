namespace LightORMTest.SqlGenerate;

[TestClass]
public class DeleteSql : TestBase
{
    [TestMethod]
    public void D1_Delete_Batch()
    {
        var datas = GetList();

        var sql = Db.Delete(datas.ToArray())
            .Where(s => s.Recive.Code == 100)
            .ToSql();
        Console.WriteLine(sql);
        List<SmsLog> GetList()
        {
            return new List<SmsLog>
            {
                new() ,
                new() ,
                new() ,
                new() ,
                new() ,
                new() ,
            };
        }
    }
    [TestMethod]
    public void D2_Delete_Force()
    {

        var sql = Db.Delete<SmsLog>(true, true)
            .ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void D3_Delete_Exits()
    {

        var sql = Db.Delete<User>()
            .Where(s => s.UserRoles.Where(ur => ur.RoleName.StartsWith("admin")).Any())
            .ToSql();
        Console.WriteLine(sql);

        sql = Db.Delete<UserRole>()
           .Where(ur => ur.User.Age > 10)
           .ToSql();
        Console.WriteLine(sql);

    }
}
