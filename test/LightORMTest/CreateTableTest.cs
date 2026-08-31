using System.Text;

namespace LightORMTest;

public class CreateTableTest : TestBase
{
    [TestMethod]
    public void GenerateCreateTableSql()
    {
        StringBuilder sql = new();
        using var db = this.Db.CreateMainDbScoped();
        sql.AppendLine(db.CreateTableSql<User>());
        sql.AppendLine();
        sql.AppendLine(db.CreateTableSql<UserRole>());
        sql.AppendLine();
        sql.AppendLine(db.CreateTableSql<Role>());
        sql.AppendLine();
        sql.AppendLine(db.CreateTableSql<RolePermission>());
        sql.AppendLine();
        sql.AppendLine(db.CreateTableSql<Permission>());
        sql.AppendLine();
        sql.AppendLine(db.CreateTableSql<UserFlat>());
        sql.AppendLine();
        sql.AppendLine(db.CreateTableSql<Product>());
        Console.WriteLine(sql.ToString());
    }
    [TestMethod]
    public async Task CreateTable()
    {
        using var db = this.Db.CreateMainDbScoped();
        await db.DropTableAsync<User>(TestContext.CancellationToken);
        await db.DropTableAsync<UserProfile>(TestContext.CancellationToken);
        await db.DropTableAsync<UserRole>(TestContext.CancellationToken);
        await db.DropTableAsync<Role>(TestContext.CancellationToken);
        await db.DropTableAsync<RolePermission>(TestContext.CancellationToken);
        await db.DropTableAsync<Permission>(TestContext.CancellationToken);
        await db.DropTableAsync<UserFlat>(TestContext.CancellationToken);
        await db.DropTableAsync<Product>(TestContext.CancellationToken);
        await db.DropTableAsync<Sales>(TestContext.CancellationToken);
        await db.CreateTableAsync<User>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<UserProfile>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<UserRole>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Role>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<RolePermission>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Permission>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<UserFlat>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Product>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Sales>(cancellationToken: TestContext.CancellationToken);

    }

    public required TestContext TestContext { get; set; }
}
