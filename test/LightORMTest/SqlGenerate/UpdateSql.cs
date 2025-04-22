using LightORMTest;

namespace LightORMTest.SqlGenerate;

[TestClass]
public class UpdateSql : TestBase
{
    [TestMethod]
    public void UpdateEntity()
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
    public void UpdateColumns()
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
    public void UpdateColumn()
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
    public void IgnoreColumn()
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
}
