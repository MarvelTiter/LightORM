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
        await db.CreateTableAsync<User>();
        await db.CreateTableAsync<UserRole>();
        await db.CreateTableAsync<Role>();
        await db.CreateTableAsync<RolePermission>();
        await db.CreateTableAsync<Permission>();
        await db.CreateTableAsync<UserFlat>();
        await db.CreateTableAsync<Product>();

    }
}
