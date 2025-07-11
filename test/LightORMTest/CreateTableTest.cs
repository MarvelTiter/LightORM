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
        sql.AppendLine(Db.CreateTableSql<User>());
        sql.AppendLine();
        sql.AppendLine(Db.CreateTableSql<UserRole>());
        sql.AppendLine();
        sql.AppendLine(Db.CreateTableSql<Role>());
        sql.AppendLine();
        sql.AppendLine(Db.CreateTableSql<RolePermission>());
        sql.AppendLine();
        sql.AppendLine(Db.CreateTableSql<Permission>());
        sql.AppendLine();
        sql.AppendLine(Db.CreateTableSql<UserFlat>());
        sql.AppendLine();
        sql.AppendLine(Db.CreateTableSql<Product>());
        Console.WriteLine(sql.ToString());
    }
    [TestMethod]
    public async Task CreateTable()
    {
        await Db.CreateTableAsync<User>();
        await Db.CreateTableAsync<UserRole>();
        await Db.CreateTableAsync<Role>();
        await Db.CreateTableAsync<RolePermission>();
        await Db.CreateTableAsync<Permission>();
        await Db.CreateTableAsync<UserFlat>();
        await Db.CreateTableAsync<Product>();

    }
}
