namespace LightORMTest.SqlGenerate;

public class DeleteSql : TestBase
{
    [TestMethod]
    public void Delete_Batch()
    {
        var datas = GetList();

        var sql = Db.Delete(datas.ToArray())
            .Where(s => s.PriInfo.Age == 100)
            .ToSql();
        Console.WriteLine(sql);
        List<UserFlat> GetList()
        {
            return new List<UserFlat>
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
    public void Delete_Force()
    {

        var sql = Db.Delete<UserFlat>()
            .FullDelete(true)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void Delete_Exits()
    {
        var name = "admin";
        var age = 10;
        var sql = Db.Delete<User>()
            .Where(s => s.UserRoles.Where(ur => ur.RoleName.StartsWith(name)).Any())
            .ToSqlWithParameters();
        Console.WriteLine(sql);

        sql = Db.Delete<UserRole>()
           .Where(ur => ur.User.Age > age)
           .ToSqlWithParameters();
        Console.WriteLine(sql);

    }
}
