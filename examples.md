- [更新日志](./doc/版本日志.md)
- [简介](#简介)
- [注册和配置](#注册和配置)
- [使用生成器(可选)](#使用生成器可选)
  - [用途](#用途)
  - [使用](#使用)
- [查询(示例代码中使用的Db均为IExpressionContext对象)](#查询示例代码中使用的db均为iexpressioncontext对象)
  - [基础查询](#基础查询)
  - [Join查询](#join查询)
  - [多表查询](#多表查询)
  - [子查询](#子查询)
  - [Join 子查询](#join-子查询)
  - [With (tempName) AS (...) 查询](#with-tempname-as--查询)
  - [Include查询](#include查询)
  - [Union 查询](#union-查询)
    - [已有查询Union新的查询](#已有查询union新的查询)
    - [使用`IExpressionContext.Union`](#使用iexpressioncontextunion)
  - [数据库函数](#数据库函数)
    - [聚合函数(使用SqlFn的静态方法调用，或者GroupBy后的方法调用中的参数`IExpSelectGrouping<TGroup, TTables>`中调用)](#聚合函数使用sqlfn的静态方法调用或者groupby后的方法调用中的参数iexpselectgroupingtgroup-ttables中调用)
    - [开窗函数(窗口函数)](#开窗函数窗口函数)
- [更新](#更新)
  - [实体更新](#实体更新)
  - [指定列更新](#指定列更新)
  - [忽略列更新](#忽略列更新)
  - [批量更新](#批量更新)
- [插入](#插入)
  - [实体插入](#实体插入)
  - [批量插入](#批量插入)
- [删除](#删除)
- [Ado对象](#ado对象)
- [事务](#事务)
- [仓储模式和Linq](#仓储模式和Linq)
- [待办](#待办)
- [复杂查询示例(Oracle)](#复杂查询示例oracle)

# 简介

无任何依赖项的轻量级的Orm工具，只负责解析`Expression`，然后拼接成Sql语句。使用`Expression`动态构建类型映射。

主体

```
dotnet add package MT.LightORM --version *
```

Provider ( `Sqlite` | `MySql` | `Oracle` | `SqlServer` | `PostgreSQL`)

```
dotnet add package LightORM.Providers.Sqlite --version *
```

# 注册和配置

```csharp
// IServiceCollection
// 注入IExpressionContext对象使用
services.AddLightOrm(option => {
    option.UseSqlite("DataSource=" + path);
})

// 直接使用
var path = Path.GetFullPath("../../../test.db");
ExpSqlFactory.Configuration(option =>
{
    option.UseSqlite("DataSource=" + path);
    option.SetTableContext(new TestTableContext());
    option.SetWatcher(aop =>
    {
        aop.DbLog = (sql, p) =>
        {
            Console.WriteLine(sql);
        };
    });//.InitializedContext<TestInitContext>();
});
IExpressionContext Db = ExpSqlFactory.GetContext();
```

# 从配置文件注册数据库

依赖 `LightORM.Extensions.DependencyInjection`。把 `IConfiguration` 传给 `AddLightOrm` 的第二个参数，
数据库改由配置节 `LightOrmDatabases` 描述；代码里只调用无参 `UseXxx()` 登记「该方言如何创建」，
不再传连接串。

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("db.json", optional: false, reloadOnChange: true)
    .Build();

services.AddLightOrm(option =>
{
    option.SetTableContext(new TestTableContext());
    // 只登记创建器：配置里出现 Type = "Sqlite" / "Dameng" 才会真正建 provider
    option.UseSqlite();
    option.UseDameng();
}, configuration);
```

```json
{
  "LightOrmDatabases": [
    {
      "Key": "MainDb",
      "Option": {
        "Type": "Sqlite",
        "DataSource": "test.db"
      }
    },
    {
      "Key": "Report",
      "Option": {
        "Type": "Dameng",
        "Server": "localhost",
        "Port": 5236,
        "UserId": "SYSDBA",
        "Password": "SYSDBA001"
      }
    }
  ]
}
```

## 配置结构

`LightOrmDatabases` 是数组，每个元素对应一个注册的库：

| 字段 | 类型 | 说明 |
|------|------|------|
| `Key` | string | 注册名（多库时用它切换）。省略则视为 `MainDb`，`DefaultDbKey` 默认也取 `MainDb` |
| `Option` | object | 库定义，缺了这条会被跳过并记一条警告日志 |
| `Option.Type` | string | **必填**，方言名。取值须与调用过的空参 `UseXxx()` 一致，大小写不敏感 |
| `Option.ConnectionString` | string | 完整连接串。同时给了分散字段时，它作为基底被覆盖 |
| `Option.DataSource` | string | Sqlite 的文件路径；Oracle 的 TNS 别名 / 完整描述符 / EZ Connect；其余方言视作主机或数据源 |
| `Option.Server` | string | 主机名。Oracle 在没有 `ServiceName` / `Sid` 时按 TNS 别名原样使用 |
| `Option.Port` | int | 端口，省略用方言默认（Oracle 1521、达梦 5236 等） |
| `Option.UserId` / `Option.Password` | string | 凭据 |
| `Option.Database` | string | 库名；Sqlite 在无 `DataSource` 时把它当文件路径 |
| `Option.ServiceName` | string | Oracle 专用，拼成 `host:port/service` |
| `Option.Sid` | string | Oracle 专用，拼成 `(DESCRIPTION=...(CONNECT_DATA=(SID=...)))`；与 `ServiceName` 二选一 |
| `Option.SlaveConnectionStrings` | string[] | 从库连接串（读写分离用） |
| `Option.IsUseIdentifierQuote` | bool | 是否引用标识符 |
| `Option.SpecificVersion` | string | 指定服务端版本；给了就不再连库探测 |
| `Option.DetectVersion` | bool | `false` 时不探测版本，直接用方言基线能力 |
| `Option.DefaultStringLength` | int | 默认字符串长度 |
| `Option.Settings` | object | 方言私有开关，只支持一层字符串键值，如 PG 的 `"TableSpace": "pg_default"` |

> 只收录「拼出可用连接串必需」的字段。连接超时、命令超时、连接池大小这类调优项请写在
> `ConnectionString` 里，或走代码方式配置。

`Option.Type` 的规范写法（大小写不敏感，但建议照抄）：
`Sqlite` / `MySql` / `SqlServer` / `PostgreSQL` / `Oracle` / `Dameng` / `KingbaseES`，
分别对应代码里的 `UseSqlite()` / `UseMySql()` / `UseSqlServer()` / `UsePostgreSQL()` /
`UseOracle()` / `UseDameng()` / `UseKingbaseES()`。

## 各方言拼串规则

| 方言 | 规则 |
|------|------|
| `Sqlite` | `DataSource` → 否则 `Database` 当文件路径；两者都没有且无 `ConnectionString` 则抛错 |
| `MySql` | `Server` / `Port` / `UserId` / `Password` / `Database` |
| `SqlServer` | `Server` 作 `Data Source`；端口折进 `Data Source=host,port`。`Server` 已含 `,` 或 `\`（命名实例）时再给 `Port` 会直接报错 |
| `PostgreSQL` / `KingbaseES` | `DataSource`（回退 `Server`）作 `Host`、`UserId` 作 `Username`、`Port` / `Password` / `Database` |
| `Dameng` | `DataSource` 优先，否则 `Server` + `Port`（默认 5236）+ 凭据 |
| `Oracle` | `DataSource` 原样透传 → `ServiceName` 拼 EZ Connect → `Sid` 拼完整描述符 → 裸 `Server` 当 TNS 别名 |

Oracle 补充：`Sid` 不会写成 `host:port/SID`。该位置在 Oracle 语法里是 **service_name**，
只有单实例且 service 恰好与 SID 同名时才「看起来能用」，RAC 下（SID=`ORCL1`、service=`ORCL`）必失败。
另外只给 `Server` + `Port` 而不给 `ServiceName` / `Sid` 会报错 —— 拼出的 `host:port` 既不能精确匹配
TNS 别名，也不是合法的 EZ Connect。

## 优先级与热重载

1. 代码里 `UseXxx(connStr)` 先登记实例；
2. 配置文件里**同 `Key`** 的条目随后覆盖它（连接串、选项都按配置走）；
3. 配置里有、代码里没登记的 `Key` 会被新增；
4. 配置里删掉的 `Key` 会在重载时移除对应 provider。

`reloadOnChange: true` 时，改连接串、加库、删库都会在文件保存后生效，无需重启。
`AddLightOrm` 不传 `configuration` 时整条链路跳过，行为与旧版本一致。

## 注意

- `Option.Type` 写了一个没登记过的方言名，会在加载时抛异常并列出当前已支持的方言名。
- 配置键名目前**不做严格校验**：拼错的键（如 `"UsrId"`）会被静默忽略，
  症状是「明明写了却连不上」。遇到这种情况先核对键名拼写。
- 手写绑定器 `BindingExtensions` 是唯一入口，新增配置字段必须同步改它，否则字段会被忽略。

# 使用生成器(可选)

## 用途

用于收集实体类型信息，以及创建读写值的方法

## 使用

```csharp
// 创建一个partial class, 例如 TestTableContext
// 标注`LightORMTableContext`Attribute
[LightORMTableContext]
public partial class TestTableContext
{

}
// 在配置的时候应用
option.SetTableContext(new TestTableContext());
```

# 查询(示例代码中使用的Db均为IExpressionContext对象)

## 基础查询

```csharp
Db.Select<Product>()
    .Where(p => p.ModifyTime > DateTime.Now)
    .ToSql(p => new { p.ProductId, p.ProductName });
```

```sql
SELECT "a"."PRODUCT_ID" AS "ProductId", "a"."PRODUCT_NAME" AS "ProductName"
FROM "PRODUCTS" "a"
WHERE ("a"."MODIFY_TIME" > @Now_0_0)
```

## Join查询

```csharp
Db.Select<User>()
    .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
    .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
    .Where(u => u.UserId == "admin")
    .ToSql(w => w.Tb1);
```

```sql
SELECT "a".*
FROM "USER" "a"
INNER JOIN "USER_ROLE" "b" ON ("a"."USER_ID" = "b"."USER_ID")
INNER JOIN "ROLE" "c" ON ("b"."ROLE_ID" = "c"."ROLE_ID")
WHERE ("a"."USER_ID" = 'admin')
```

## 多表查询

```csharp
Db.Select<Permission, RolePermission, Role>()
   .Distinct()
   .Where(w => w.Tb1.PermissionId == w.Tb2.PermissionId && w.Tb2.RoleId == w.Tb3.RoleId)
   .ToSql(w => new { w.Tb1 });
```

```sql
SELECT DISTINCT "a".*
FROM "PERMISSIONS" "a", "ROLE_PERMISSION" "b", "ROLE" "c"
WHERE (("a"."PERMISSION_ID" = "b"."POWER_ID") AND ("b"."ROLE_ID" = "c"."ROLE_ID"))
```

## 子查询

```csharp
Db.Select<User>()
    .Where(u => u.Age > 10)
    .GroupBy(u => u.UserId )
    .AsTable(u => new
    {
        u.Group.UserId,
        Total = u.Count()
    }).AsSubQuery()
    .Where(t => t.UserId.Contains("admin"))
    .ToSql();
```

```sql
SELECT *
FROM (
    SELECT "a"."USER_ID" AS "UserId", COUNT(*) AS "Total"
    FROM "USER" "a"
    WHERE ("a"."AGE" > 10)
    GROUP BY "a"."USER_ID"
) "a"
WHERE "a"."UserId" LIKE '%' || 'admin'|| '%'
```

## Join 子查询

```csharp
Db.Select<User>()
    .LeftJoin(Db.Select<Product>().GroupBy(p => new { p.ProductId })ToSelect(g => new
    {
        g.Group.ProductId,
        Total = g.Count()
    }), (u, j) => u.Age == j.ProductId)
    .Where(w => w.Tb2.Total > 10)
    .ToSql();
```

```sql
SELECT *
FROM "USER" "a"
LEFT JOIN (
    SELECT "a"."PRODUCT_ID" AS "ProductId", COUNT(*) AS "Total"
    FROM "PRODUCTS" "a"
    GROUP BY "a"."PRODUCT_ID"
) "b" ON ("a"."AGE" = "b"."ProductId")
WHERE ("b"."Total" > 10)
```

## With (tempName) AS (...) 查询

```csharp
var tempU = Db.Select<User>().GroupBy(u => new { u.UserId }).AsTemp("us", g => new
    {
        g.Group.UserId,
        Total = g.Count()
    });
var tempR = Db.Select<Role>().WithTempQuery(tempU)
    .Where((r, u) => r.RoleId == u.UserId)
    .Where(w => w.Tb2.UserId.StartsWith("ad"))
    .AsTemp("temp", w => new
    {
        w.Tb1.RoleId,
        w.Tb2.UserId,
    });

var tempT = Db.FromTemp(tempR).Where(a => a.RoleId.StartsWith("h")).AsTemp("hh");

var sql = Db.Select<Permission>().WithTempQuery(tempT)
    .Where(w => w.Tb2.UserId.Contains("admin"))
    .ToSql();
```

```sql
WITH us AS (
    SELECT "a"."USER_ID" AS "UserId", COUNT(*) AS "Total"
    FROM "USER" "a"
    GROUP BY "a"."USER_ID"
)
, temp AS (
    SELECT "a"."ROLE_ID" AS "RoleId", "b"."UserId"
    FROM "ROLE" "a", us "b"
    WHERE ("a"."ROLE_ID" = "b"."UserId") AND "b"."UserId" LIKE 'ad'||'%'
)
, hh AS (
    SELECT *
    FROM temp "a"
    WHERE "a"."RoleId" LIKE 'h'||'%'
)
SELECT *
FROM "PERMISSIONS" "a", hh "b"
WHERE "b"."UserId" LIKE '%' || 'admin'|| '%'
```

## Include查询

需要配置导航关系

```csharp
// 写法1，使用自定义扩展方法WhereIf
var sql = Db.Select<User>()
    .Where(u => u.UserRoles.WhereIf(r => r.RoleId.Contains("admin")))
    .ToSql();

// 写法2，使用Any()
var sql = Db.Select<User>()
    .Where(u => u.UserRoles.Where(r => r.RoleId.Contains("admin")).Any())
    .ToSql();
```

```sql
SELECT DISTINCT *
FROM "USER" "a"
INNER JOIN "USER_ROLE" "b" ON ( "a"."USER_ID" = "b"."USER_ID" )
INNER JOIN "ROLE" "c" ON ( "c"."ROLE_ID" = "b"."ROLE_ID" )
WHERE "c"."ROLE_ID" LIKE '%' || 'admin'|| '%'
```

## Union 查询

### 已有查询Union新的查询

```csharp
Db.Select<User>()
    .Where(u => u.Age > 10)
    .Union(Db.Select<User>().Where(u => u.Age > 15))
    .ToSql();
```

```sql
SELECT *
FROM "USER" "a"
WHERE ("a"."AGE" > 10)
UNION
SELECT *
FROM "USER" "a"
WHERE ("a"."AGE" > 15)
```

### 使用`IExpressionContext.Union`

```csharp
Db.Union(Db.Select<User>(), Db.Select<User>())
    .Where(u => u.Age > 10)
    .ToSql();
```

```sql
SELECT *
FROM (
    SELECT *
    FROM "USER" "a"
    UNION
    SELECT *
    FROM "USER" "a"
) "a"
WHERE ("a"."AGE" > 10)
```

## 数据库函数

### 聚合函数(使用SqlFn的静态方法调用，或者GroupBy后的方法调用中的参数`IExpSelectGrouping<TGroup, TTables>`中调用)

COUNT(*)

COUNT<T>(T column), 当 column 是(一个二元表达式，并且T的类型是bool) 或者 是一个三元表达式, 会解析成 CASE WHEN 语句

MAX、MIN、AVG、SUM

Join(在分组数据中拼接字符串，不同数据库，调用的函数不同，例如，mysql是group_concat， oracle是listagg等)

### 开窗函数(窗口函数)

RowNumber、Lag、Rank

支持对导航属性的解析（仅支持1对1的导航属性

```csharp
Db.Select<User>()
    .ToSql(u => new
    {
        RowNo = WinFn.RowNumber().PartitionBy(u.City.Name).OrderBy(u.Id).Value(),
        u.UserId,
        u.UserName,
        u.Password
    });

```

```sql
SELECT ROW_NUMBER() OVER( PARTITION BY "b"."Name" ORDER BY "a"."ID" ASC ) AS "RowNo", "a"."USER_ID" AS"UserId", "a"."USER_NAME" AS "UserName", "a"."PASSWORD" AS "Password"
FROM "USER" "a"
INNER JOIN "City" "b" ON ( "a"."ID" = "b"."Id" )
```

# 更新

## 实体更新

根据配置的主键更新实体，并且忽略null值

```csharp
Db.Update(p).ToSql();
```

```sql
UPDATE `Product` SET
`CategoryId` = @CategoryId,
`ProductCode` = @ProductCode,
`ProductName` = @ProductName,
`DeleteMark` = @DeleteMark,
`CreateTime` = @CreateTime,
`Last` = @Last
WHERE `ProductId` = @ProductId
```

## 指定列更新

```csharp
Db.Update<Product>()
    .UpdateColumns(() => new { p.ProductName, p.CategoryId })
    .Where(p => p.ProductId > 10)
    .ToSql()
```

```sql
UPDATE `Product` SET
`CategoryId` = @CategoryId,
`ProductName` = @ProductName
WHERE ( `ProductId` > 10 )
```

## 忽略列更新

```csharp
Db.Update(p)
    .IgnoreColumns(p => new { p.ProductName, p.CategoryId })
    .ToSql();
```

```sql
UPDATE `Product` SET
`ProductCode` = @ProductCode,
`DeleteMark` = @DeleteMark,
`CreateTime` = @CreateTime,
`Last` = @Last
WHERE `ProductId` = @ProductId
```

## 批量更新

# 插入

## 实体插入

```csharp
Db.Insert(p).ToSql();
```

```sql
INSERT INTO `Product` 
(`ProductId`, `CategoryId`, `ProductCode`, `ProductName`, `DeleteMark`, `CreateTime`, `Last`) 
VALUES 
(@ProductId, @CategoryId, @ProductCode, @ProductName, @DeleteMark, @CreateTime, @Last)
```

## 批量插入

# 删除

# JSON列

支持JSON列的插入、查询、更新，需要配置`ILightJsonHelper`
```csharp
services.AddLightOrm(option =>
{
    // 方式一：配置实例
    option.ConfigJsonHandler<JsonHandler>();
    // 方式二：配置委托
    option.ConfigJsonHandler(c =>
    {
        c.Serializer = 序列化委托;
        c.Deserializer = 反序列化委托（参数是string）;
        c.DeserializerBytes = 反序列化委托（参数是byte[]）;
    });
});
```

## JSON对象查询(Sqlite演示)
```csharp
Db.Select<JsonTestModel>()
    // JSON_EXTRACT(a.`JSON_DATA`,'$.NestJson.Name') = 'test'
    .Where(j => j.Json!.NestJson!.Name == "test")
    .ToSqlWithParameters();
```
```sql
SELECT *
FROM `JSON_TEST` a
WHERE (JSON_EXTRACT(a.`JSON_DATA`,'$.NestJson.Name') = 'test')
```
## JSON对象更新(Sqlite演示)
```csharp
Db.Update<JsonTestModel>()
    .Set(j => j.Json!.NestJson!.Name, "test")
    .Where(j => j.Id == 5)
    .ToSqlWithParameters();
```
```sql
UPDATE `JSON_TEST` SET   
`JSON_DATA` = JSON_SET(`JSON_DATA`,'$.NestJson.Name',@Json)
WHERE (`ID` = 5)

参数列表: 
Json - test
```
# Ado对象

直接执行sql语句, 可返回`IEnumerable<T>`,`IAsyncEnumerable<T>`,`DataTable`,`DataReader`等等

# 事务

使用`IExpressionContext.CreateScoped`创建`IScopedExpressionContext`或者`ISingleScopedExpressionContext`对象，提供了BeginTransaction、CommitTransaction、RollbackTransaction以及对应的异步版本，其中`IScopedExpressionContext`支持切换数据库，CommitTransaction+RollbackTransaction的次数必须等于BeginTransaction的次数

```csharp
using var scoped = Db.CreateScoped("MainDb");
var role = new Role()
{
    RoleId = "TT001",
    RoleName = "测试",
    Powers = pp.Select(p => new Power() { PowerId = p })
};
try
{
    await scoped.BeginTransactionAsync();
    var n = await scoped.Update(role)
        .Where(r => r.RoleId == role.RoleId)
        .ExecuteAsync();
    var roleId = role.RoleId;
    string[] powers = [.. role.Powers.Select(p => p.PowerId).Distinct()];
    var d = await scoped.Delete<RolePower>().Where(r => r.RoleId == roleId).ExecuteAsync();
    var i = 0;
    
    var ef = await scoped.Insert<RolePower>(powers.Select(p => new RolePower()
    {
        RoleId = roleId,
        PowerId = p
    })).ExecuteAsync();
    i += ef;
    if (powers.Length == 3)
    {
        await scoped.CommitTransactionAsync();
    }
    else
    {
        await scoped.RollbackTransactionAsync();
    }
}
catch
{
    await scoped.RollbackTransactionAsync();
}
```

```csharp
using var scoped = Db.CreateScoped();
await scoped.BeginTransactionAsync("v");
await scoped.Select<UserRole>().ToListAsync();
await scoped.SwitchDatabase("v").Select<UserRole>().ToListAsync();
await scoped.CommitTransactionAsync("v");
```

# 仓储模式和Linq

## 接口`ILightOrmRepository<User>`

提供对单个表的增删改查的直接操作

## Linq支持

```csharp
var usrRepo = Services.GetRequiredService<ILightOrmRepository<User>>();
var proRepo = Services.GetRequiredService<ILightOrmRepository<Product>>();
var list = from u in usrRepo.Table
           join p in proRepo.Table on u.UserId equals p.ProductCode
           group u by u.Age into g
           select new { g.Key, Count = g.Average(u => u.Age) };

```

# 复杂查询示例(Oracle)

示例数据库表

```csharp
class Jobs
{
    public string? Plate { get; set; }
    public string? StnId { get; set; }
}
```

C#代码

```csharp
// 从Jobs表中，选择Plate的第一个字符作为Fzjg字段，选择Fzjg和StnId，作为temp表，并命名为info
// With info as (...)
var info = Db.Select<Jobs>().AsTemp("info", j => new
{
    Fzjg = j.Plate!.Substring(1, 2),
    j.StnId
});
// 从info表中，按StnId和Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名stn_fzjg，表数据为每个StnId中，按Fzjg数据量进行排序并标记为Index
var stnFzjg = Db.FromTemp(info)
    .GroupBy(a => new { a.StnId, a.Fzjg })
    .OrderByDesc(a => new { a.Group.StnId, i = a.Count() })
    .AsTemp("stn_fzjg", g => new
    {
        g.Group.StnId,
        g.Group.Fzjg,
        Count = g.Count(),
        Index = WinFn.RowNumber().PartitionBy(g.Tables.StnId).OrderByDesc(g.Count()).Value()
    });
// 从info表中，按Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名all_fzjg，表数据为所有Fzjg中，按每个Fzjg的数据量进行排序并标记为Index
var allFzjg = Db.FromTemp(info).GroupBy(a => new { a.Fzjg })
    .OrderByDesc(a => a.Count())
    .AsTemp("all_fzjg", g => new
    {
        StnId = "合计",
        g.Group.Fzjg,
        Count = g.Count(),
        Index = WinFn.RowNumber().OrderByDesc(g.Count()).Value()
    });
// 从info表中，按StnId进行Group By Rollup ，选择StnId和分组数据量，作为temp表，并命名为all_station
var allStation = Db.FromTemp(info).GroupBy(t => new { t.StnId })
    .Rollup()
    .AsTemp("all_station", g => new
    {
        StnId = SqlFn.NullThen(g.Group.StnId, "合计"),
        Total = SqlFn.Count()
    });
/*
 * 1. 从stn_fzjg中，筛选出所有前3的Fzjg数量，然后按StnId分组，选择StnId，组内第一Fzjg作为FirstFzjg，组内第一Count作为FirstCount
 * 2. 从all_fzjg中，筛选出所有前3的Fzjg数量，选择'合计'作为StnId，第一Fzjg作为FirstFzjg，第一的Count作FirstCount
 * 3. 将1和2的结果Union ALl
 * 4. 转为子查询，inner join all_station
 * 5. select结果列
 */
var sql = Db.FromTemp(stnFzjg).Where(t => t.Index < 4)
    .GroupBy(t => new { t.StnId })
    .AsTable(g => new
    {
        StnId = g.Group.StnId!,
        FirstFzjg = g.Join(g.Tables.Index == 1 ? g.Tables.Fzjg.ToString() : "").Separator("").OrderB(g.Tables.StnId).Value(),
        FirstCount = g.Join(g.Tables.Index == 1 ? g.Tables.Count.ToString() : "").Separator("").OrderB(g.Tables.StnId).Value()
    }).UnionAll(Db.FromTemp(allFzjg).Where(t => t.Index < 4).AsTable(g => new
    {
        StnId = "合计",
        FirstFzjg = SqlFn.Join(g.Index == 1 ? g.Fzjg.ToString() : "").Separator("").OrderB(g.StnId).Value(),
        FirstCount = SqlFn.Join(g.Index == 1 ? g.Count.ToString() : "").Separator("").OrderB(g.StnId).Value()
    })).AsSubQuery()
    .InnerJoin(allStation, (t, a) => t.StnId == a.StnId)
    .ToSql((t, a) => new
    {
        Jczmc = SqlFn.NullThen(t.StnId, "TT"),
        a.Total,
        t
    });
Console.WriteLine(sql);
```

```sql
    WITH info AS (
    SELECT SUBSTR("a"."Plate",1,2) AS "Fzjg", "a"."StnId"
    FROM "Jobs" "a"
)
, stn_fzjg AS (
    SELECT "a"."StnId", "a"."Fzjg", COUNT(*) AS "Count", ROW_NUMBER() OVER( PARTITION BY "a"."StnId" ORDER BY COUNT(*) DESC ) AS "Index"
    FROM info "a"
    GROUP BY "a"."StnId", "a"."Fzjg"
    ORDER BY "a"."StnId", COUNT(*) DESC
)
, all_fzjg AS (
    SELECT '合计' AS "StnId", "a"."Fzjg", COUNT(*) AS "Count", ROW_NUMBER() OVER( ORDER BY COUNT(*) DESC ) AS "Index"
    FROM info "a"
    GROUP BY "a"."Fzjg"
    ORDER BY COUNT(*) DESC
)
, all_station AS (
    SELECT NVL("a"."StnId",'合计') AS "StnId", COUNT(*) AS "Total"
    FROM info "a"
    GROUP BY ROLLUP ("a"."StnId")
)
SELECT NVL("a"."StnId",'TT') AS "Jczmc", "b"."Total", "a".*
FROM (
    SELECT "a"."StnId", LISTAGG( CASE WHEN ("a"."Index" = 1) THEN TO_CHAR("a"."Fzjg") ELSE '' END, '') WITHIN GROUP (ORDER BY "a"."StnId" ASC) AS "FirstFzjg", LISTAGG( CASE WHEN ("a"."Index" = 1) THEN TO_CHAR("a"."Count") ELSE '' END, '') WITHIN GROUP (ORDER BY "a"."StnId" ASC) AS "FirstCount"
    FROM stn_fzjg "a"
    WHERE ("a"."Index" < 4)
    GROUP BY "a"."StnId"
    UNION ALL
    SELECT '合计' AS "StnId", LISTAGG( CASE WHEN ("a"."Index" = 1) THEN TO_CHAR("a"."Fzjg") ELSE '' END, '') WITHIN GROUP (ORDER BY "a"."StnId" ASC) AS "FirstFzjg", LISTAGG( CASE WHEN ("a"."Index" = 1) THEN TO_CHAR("a"."Count") ELSE '' END, '') WITHIN GROUP (ORDER BY "a"."StnId" ASC) AS "FirstCount"
    FROM all_fzjg "a"
    WHERE ("a"."Index" < 4)
) "a"
INNER JOIN all_station "b" ON ("a"."StnId" = "b"."StnId")
```

