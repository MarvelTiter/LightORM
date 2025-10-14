namespace LightORMTest.SqlGenerate;

public partial class SelectSql
{
    [TestMethod]
    public void TestLeftJoin()
    {
        var sql = Db.Select<User>()
            .LeftJoin<UserRole>((u,r) => u.UserId == r.UserId)
            .Where(u => u.Age > 10)
            .ToSql();
        Console.WriteLine(sql);
        AssertSqlResult(nameof(TestLeftJoin), sql);
    }
    [TestMethod]
    public void TestInnerJoin()
    {
        var sql = Db.Select<User>()
            .InnerJoin<UserRole>((u, r) => u.UserId == r.UserId)
            .Where(u => u.Age > 10)
            .ToSql();
        Console.WriteLine(sql);
        AssertSqlResult(nameof(TestInnerJoin), sql);
    }
    [TestMethod]
    public void TestRightJoin()
    {
        var sql = Db.Select<User>()
            .RightJoin<UserRole>((u, r) => u.UserId == r.UserId)
            .Where(u => u.Age > 10)
            .ToSql();
        Console.WriteLine(sql);
        AssertSqlResult(nameof(TestRightJoin), sql);
    }
    [TestMethod]
    public void TestOuterJoin()
    {
        var sql = Db.Select<User>()
            .OuterJoin<UserRole>((u, r) => u.UserId == r.UserId)
            .Where(u => u.Age > 10)
            .ToSql();
        Console.WriteLine(sql);
        AssertSqlResult(nameof(TestOuterJoin), sql);
    }
}