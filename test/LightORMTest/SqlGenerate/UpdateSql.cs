using LightORMTest;

namespace LightORMTest.SqlGenerate;

[TestClass]
public class UpdateSql : TestBase
{
    [TestMethod]
    public void U01_UpdateEntity()
    {
        var p = new Product();
        var sql = Db.Update(p).ToSql();
        Console.WriteLine(sql);
        var result = """
            UPDATE `Product` SET
            `CategoryId` = @CategoryId,
            `ProductCode` = @ProductCode,
            `ProductName` = @ProductName,
            `DeleteMark` = @DeleteMark,
            `CreateTime` = @CreateTime,
            `Last` = @Last
            WHERE (`ProductId` = @ProductId)
            """;
        Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void U02_UpdateColumns()
    {
        var p = new Product();
        var sql = Db.Update<Product>()
            .UpdateColumns(() => new { p.ProductName, p.CategoryId })
            .Where(p => p.ProductId > 10)
            .ToSql();
        Console.WriteLine(sql);
        var result = """
            UPDATE `Product` SET
            `CategoryId` = @CategoryId,
            `ProductName` = @ProductName
            WHERE (`ProductId` > 10)
            """;
        Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void U03_UpdateColumn()
    {
        var p = new Product();
        var sql = Db.Update<Product>()
            .Set(p => p.ProductName, p.ProductName)
            .SetNull(p => p.ProductCode)
            .Where(p => p.ProductId > 10)
            .ToSql();
        Console.WriteLine(sql);
        var result = """
            UPDATE `Product` SET
            `ProductName` = @ProductName,
            `ProductCode` = NULL
            WHERE (`ProductId` > 10)
            """;
        Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void U04_IgnoreColumn()
    {
        var p = new Product();
        var sql = Db.Update(p)
            .IgnoreColumns(p => new { p.ProductName, p.CategoryId })
            .ToSql();
        Console.WriteLine(sql);
        var result = """
            UPDATE `Product` SET
            `ProductCode` = @ProductCode,
            `DeleteMark` = @DeleteMark,
            `CreateTime` = @CreateTime,
            `Last` = @Last
            WHERE (`ProductId` = @ProductId)
            """;
        Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void U05_Update_Flat_Column()
    {
        var p = new SmsLog();
        var sql = Db.Update(p)
            .Set(s => s.Recive.Code, 100)
            .Where(s => s.Recive.Uuid == "123")
            .ToSql();
        Console.WriteLine(sql);
        var result = """
            UPDATE `SMS_LOG` SET
            `CODE` = @Code
            WHERE (`UUID` = '123')
            """;
        Assert.IsTrue(sql == result);
    }

    [TestMethod]
    public void U06_Update_Flat_Entity()
    {
        var p = new SmsLog();
        var sql = Db.Update(p)
            .ToSql();
        Console.WriteLine(sql);
        var result = """
            UPDATE `SMS_LOG` SET
            `CODE` = @Code,
            `MSG` = @Msg,
            `CREATE_TIME` = @CreateTime
            WHERE (`UUID` = @Uuid)
            """;
        Assert.IsTrue(sql == result);
    }
}
