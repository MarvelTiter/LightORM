# MT.LightORM - 轻量级.NET ORM工具

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/MT.LightORM.svg)](https://www.nuget.org/packages/MT.LightORM)

## 🌟 项目简介

**MT.LightORM** 是一款极致轻量的 .NET ORM 工具，核心设计理念是**零依赖、高效率、易使用**。它专注于做好一件事：将 `Expression` 表达式树解析为 SQL 语句，不包含任何复杂的数据映射或状态管理逻辑。

> ⚡ **核心优势**：无任何第三方依赖，纯.NET标准库实现，即插即用

## ✨ 主要特点

### 🎯 极致轻量
- **零依赖**：核心库不依赖任何第三方组件
- **按需使用**：仅解析表达式树，不涉及数据映射、变更追踪等重量级功能
- **性能优异**：表达式树动态编译，运行时无反射开销

### 🔧 简单配置
```csharp
// 三步完成配置
services.AddLightOrm(option => {
    option.UseSqlite("DataSource=test.db")  // 选择数据库
          .SetTableContext(new TestTableContext())  // 设置实体上下文
          .SetWatcher(aop => aop.DbLog = (sql, p) => Console.WriteLine(sql)); // 开启日志
});

// 或直接使用
IExpressionContext Db = ExpSqlFactory.GetContext();
```

### 📦 功能齐全

| 功能特性 | 支持情况 | 说明 |
|---------|---------|------|
| **基础CRUD** | ✅ | 单表/多表操作 |
| **复杂查询** | ✅ | Join/Union/子查询/CTE |
| **导航属性** | ✅ | Include/Any自动处理关联 |
| **JSON列** | ✅ | 支持JSON序列化/反序列化 |
| **窗口函数** | ✅ | RowNumber/Lag/Rank等 |
| **批量操作** | ✅ | 批量插入/更新 |
| **仓储模式** | ✅ | 提供泛型仓储接口 |
| **LINQ支持** | ✅ | 完整的LINQ表达式支持 |

## 🚀 快速上手

### 实体配置（可选，推荐使用）

使用源生成器自动收集实体信息，避免运行时反射：

```csharp
[LightORMTableContext]
public partial class TestTableContext
{
    // 自动生成实体映射代码
}
```

### 基础查询示例

```csharp
var db = ExpSqlFactory.GetContext();

// 基础查询
var products = db.Select<Product>()
    .Where(p => p.ModifyTime > DateTime.Now)
    .ToSql(p => new { p.ProductId, p.ProductName });

// 多表Join
var users = db.Select<User>()
    .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
    .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
    .Where(u => u.UserId == "admin")
    .ToList();

// 导航属性查询
var admins = db.Select<User>()
    .Where(u => u.UserRoles.Any(r => r.RoleId.Contains("admin")))
    .ToList();

// 子查询
var subQuery = db.Select<User>()
    .GroupBy(u => u.UserId)
    .AsTable(g => new { g.Group.UserId, Total = g.Count() })
    .AsSubQuery()
    .Where(t => t.UserId.Contains("admin"));
```

### JSON列支持

```csharp
// 配置JSON处理器
option.ConfigJsonHandler<JsonHandler>();

// 查询JSON字段
var result = db.Select<JsonTestModel>()
    .Where(j => j.Json!.NestJson!.Name == "test")
    .ToList();

// 更新JSON字段
db.Update<JsonTestModel>()
    .Set(j => j.Json!.NestJson!.Name, "test")
    .Where(j => j.Id == 5)
    .Execute();
```

### CTE（公用表表达式）示例

```csharp
var temp = db.Select<User>()
    .GroupBy(u => new { u.UserId })
    .AsTemp("us", g => new { g.Group.UserId, Total = g.Count() });

var result = db.Select<Role>()
    .WithTempQuery(temp)
    .Where((r, u) => r.RoleId == u.UserId)
    .ToList();
```

### 仓储模式

```csharp
public class UserService
{
    private readonly ILightOrmRepository<User> _userRepo;
    
    public UserService(ILightOrmRepository<User> userRepo)
    {
        _userRepo = userRepo;
    }
    
    public async Task<List<User>> GetAdults()
    {
        return await _userRepo.Table
            .Where(u => u.Age >= 18)
            .ToListAsync();
    }
}
```

## 📊 支持的数据库

| 数据库 | NuGet包 | 版本 |
|-------|---------|------|
| SQLite | `LightORM.Providers.Sqlite` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.Sqlite.svg)](https://www.nuget.org/packages/LightORM.Providers.Sqlite) |
| MySQL | `LightORM.Providers.MySql` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.MySql.svg)](https://www.nuget.org/packages/LightORM.Providers.MySql) |
| Oracle | `LightORM.Providers.Oracle` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.Oracle.svg)](https://www.nuget.org/packages/LightORM.Providers.Oracle) |
| SQL Server | `LightORM.Providers.SqlServer` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.SqlServer.svg)](https://www.nuget.org/packages/LightORM.Providers.SqlServer) |
| PostgreSQL | `LightORM.Providers.PostgreSQL` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.PostgreSQL.svg)](https://www.nuget.org/packages/LightORM.Providers.PostgreSQL) |
| Dameng | `LightORM.Providers.Dameng` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.Dameng.svg)](https://www.nuget.org/packages/LightORM.Providers.Dameng) |

## 📦 安装

```bash
# 核心库
dotnet add package MT.LightORM

# 选择数据库驱动（以SQLite为例）
dotnet add package LightORM.Providers.Sqlite
```

## 📖 详细文档

- [更新日志](./doc/版本日志.md)
- [完整示例](./examples.md)

## 🎯 适用场景

- **微服务**：轻量级，适合服务拆分
- **中小型项目**：快速开发，简单配置
- **工具类应用**：无需复杂ORM功能

## 🤝 参与贡献

欢迎提交 Issue 和 PR！

## 📄 开源协议

[MIT License](LICENSE)