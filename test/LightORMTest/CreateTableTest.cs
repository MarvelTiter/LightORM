using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        await db.DropTableAsync<User>();
        await db.DropTableAsync<UserRole>();
        await db.DropTableAsync<Role>();
        await db.DropTableAsync<RolePermission>();
        await db.DropTableAsync<Permission>();
        await db.DropTableAsync<UserFlat>();
        await db.DropTableAsync<Product>();
        await db.CreateTableAsync<User>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<UserRole>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Role>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<RolePermission>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Permission>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<UserFlat>(cancellationToken: TestContext.CancellationToken);
        await db.CreateTableAsync<Product>(cancellationToken: TestContext.CancellationToken);

    }

    public TestContext TestContext { get; set; }
}
