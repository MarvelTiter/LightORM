using LightORM.SqlExecutor;
using LightORMTest;
using System.Threading.Tasks;

namespace LightORMTest.SqlGenerate;

public class UpdateSql : TestBase
{
    [TestMethod]
    public void UpdateEntity()
    {
        var p = new User()
        {
            Id = 1,
            UserId = "test01",
            UserName = "Test",
            Age = 19,
            IsLock = true,
            LastLogin = DateTime.Now,
            ModifyTime = DateTime.Now,
            Password = "Test",
            Sign = SignType.Svip,
        };
        var sql = Db.Update(p).ToSqlWithParameters();
        Console.WriteLine(sql);
        //var result = """
        //    UPDATE `Product` SET
        //    `CategoryId` = @CategoryId,
        //    `ProductCode` = @ProductCode,
        //    `ProductName` = @ProductName,
        //    `DeleteMark` = @DeleteMark,
        //    `CreateTime` = @CreateTime,
        //    `Last` = @Last
        //    WHERE (`ProductId` = @ProductId)
        //    """;
        //Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void UpdateColumns()
    {
        var p = new Product();
        var sql = Db.Update<Product>()
            .UpdateColumns(() => new { p.ProductName, p.CategoryId })
            .Where(p => p.ProductId > 10)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    UPDATE `Product` SET
        //    `CategoryId` = @CategoryId,
        //    `ProductName` = @ProductName
        //    WHERE (`ProductId` > 10)
        //    """;
        //Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void UpdateColumn()
    {
        var p = new Product();
        var sql = Db.Update<Product>()
            .Set(p => p.ProductName, p.ProductName)
            .SetNull(p => p.ProductCode)
            .Where(p => p.ProductId > 10)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    UPDATE `Product` SET
        //    `ProductName` = @ProductName,
        //    `ProductCode` = NULL
        //    WHERE (`ProductId` > 10)
        //    """;
        //Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void IgnoreColumn()
    {
        var p = new Product();
        var sql = Db.Update(p)
            .IgnoreColumns(p => new { p.ProductName, p.CategoryId })
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    UPDATE `Product` SET
        //    `ProductCode` = @ProductCode,
        //    `DeleteMark` = @DeleteMark,
        //    `CreateTime` = @CreateTime,
        //    `Last` = @Last
        //    WHERE (`ProductId` = @ProductId)
        //    """;
        //Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void Update_Flat_Column()
    {
        var p = new UserFlat();
        var sql = Db.Update(p)
            .Set(s => s.PriInfo.Age, 100)
            .Where(s => s.PriInfo.Address == "123")
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    UPDATE `SMS_LOG` SET
        //    `CODE` = @Code,
        //    `VERSION` = @Version_new
        //    WHERE (`UUID` = '123') AND (`VERSION` = @Version)
        //    """;
        //Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void Update_Flat_Entity()
    {
        var p = new UserFlat();
        var sql = Db.Update(p)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    UPDATE `SMS_LOG` SET
        //    `CODE` = @Code,
        //    `MSG` = @Msg,
        //    `CREATE_TIME` = @CreateTime,
        //    `VERSION` = @Version_new
        //    WHERE (`ID` = @Id) AND (`UUID` = @Uuid) AND (`VERSION` = @Version)
        //    """;
        //Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void Update_With_Version()
    {
        var p = new UserFlat();
        var sql = Db.Update(p)
            .ToSql();
        Console.WriteLine(sql);

    }

    [TestMethod]
    public void Update_Batch()
    {
        var datas = GetList();
        var sign = "222";
        var sql = Db.Update([.. datas])
            .Set(u => u.Sign, sign)
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
}
