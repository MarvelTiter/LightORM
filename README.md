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
- [待办](#待办)
- [复杂查询示例(Oracle)](#复杂查询示例oracle)

# 简介

无任何依赖项的轻量级的Orm工具，只负责解析`Expression`，然后拼接成Sql语句。使用`Expression`动态构建类型映射。

主体

```
dotnet add package MT.LightORM --version *
```

Provider ( `Sqlite` | `MySql` | `Oracle` | `SqlServer` )

```
dotnet add package LightORM.Providers.Sqlite --version *
```

[更新日志](./doc/版本日志.md)

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
SELECT `a1`.`ProductId`, `a1`.`ProductName`
FROM `Product` `a1`
WHERE ( `a1`.`ModifyTime` > @Now_0 )
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
SELECT `a5`.*
FROM `USER` `a5`
INNER JOIN `USER_ROLE` `a6` ON ( `a5`.`USER_ID` = `a6`.`USER_ID` )
INNER JOIN `ROLE` `a2` ON ( `a6`.`ROLE_ID` = `a2`.`ROLE_ID` )
WHERE ( `a5`.`USER_ID` = @Const_0 )
```

## 多表查询

```csharp
Db.Select<Power, RolePower, Role>()
    .Distinct()
    .Where(w => w.Tb1.PowerId == w.Tb2.PowerId && w.Tb2.RoleId == w.Tb3RoleId)
    .ToSql(w => new { w.Tb1 });
```

```sql
SELECT DISTINCT `a0`.*
FROM `POWERS` `a0`, `ROLE_POWER` `a3`, `ROLE` `a2`
WHERE ( ( `a0`.`POWER_ID` = `a3`.`POWER_ID` ) AND ( `a3`.`ROLE_ID` = `a2`.`ROLE_ID` ) )
```

## 子查询

```csharp
Db.Select<User>().Where(u => u.Age > 10).GroupBy(u => new
    {
        u.UserId
    }).AsTable(u => new
    {
        u.Group.UserId,
        Total = u.Count()
    }).AsSubQuery("temp")
    .Where(t => t.UserId.Contains("admin"))
    .ToSql();
```

```sql
SELECT *
FROM (
    SELECT `a5`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`
    FROM `USER` `a5`
    WHERE (`a5`.`AGE` > 10)
    GROUP BY `a5`.`USER_ID`
) `temp`
WHERE `temp`.`UserId` LIKE '%'||'admin'||'%'
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
FROM `USER` `a5`
LEFT JOIN (
    SELECT `a1`.`ProductId`, COUNT(*) AS `Total`
    FROM `Product` `a1`
    GROUP BY `a1`.`ProductId`
) `temp0` ON ( `a5`.`AGE` = `temp0`.`ProductId` )
WHERE ( `temp0`.`Total` > 10 )
```

## With (tempName) AS (...) 查询

```csharp
var tempU = Db.Select<User>().GroupBy(u => new { u.UserId }).ToSelect(g => new
{
    g.Group.UserId,
    Total = g.Count()
}).AsTemp("us");

var tempR = Db.Select<Role>().WithTempQuery(tempU)
    .Where((r, u) => r.RoleId == u.UserId)
    .Where(w=> w.Tb2.UserId.StartsWith("ad"))
    .AsTemp("temp",w=>new
    {
        w.Tb1.RoleId,
        w.Tb2.UserId,
    });

var sql = Db.Select<Power>().WithTempQuery(tempU, tempR)
    .Where(w => w.Tb2.Total > 10 || w.Tb3.UserId.Contains("admin"))
    .ToSql();
```

```sql
WITH us AS (
    SELECT `a5`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`
    FROM `USER` `a5`
    GROUP BY `a5`.`USER_ID`
)
,temp AS (
    SELECT `a2`.`ROLE_ID` AS `RoleId`, `temp0`.`UserId`
    FROM `ROLE` `a2`, `us` `temp0`
    WHERE ( `a2`.`ROLE_ID` = `temp0`.`UserId` ) AND `temp0`.`UserId` LIKE @temp_Const_0||'%'
)
SELECT *
FROM `POWERS` `a0`, `us` `temp0`, `temp` `temp1`
WHERE ( ( `temp0`.`Total` > 10 ) OR `temp1`.`UserId` LIKE '%'||@Const_0||'%' )
```

## Include查询

需要配置导航关系

```csharp
Db.Select<User>()
    .Where(u => u.UserRoles.When(r => r.RoleId.StartsWith("ad")))
    .ToSql();
```

```sql
SELECT DISTINCT *
FROM `USER` `a5`
LEFT JOIN `USER_ROLE` `a6` ON ( `a5`.`USER_ID` = `a6`.`USER_ID` )
LEFT JOIN `ROLE` `a2` ON ( `a2`.`ROLE_ID` = `a6`.`ROLE_ID` )
WHERE `a2`.`ROLE_ID` LIKE @Const_0||'%'
```

## Union 查询

### 已有查询Union新的查询

```csharp
Db.Select<User>().Union(Db.Select<User>())
    .Where(u => u.Age > 10)
    .ToSql();
```

```sql
SELECT *
FROM (
    SELECT *
    FROM `USER` `a5`
    UNION
    SELECT *
    FROM `USER` `a5`
) `a5`
WHERE ( `a5`.`AGE` > 10 )
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
    FROM `USER` `a5`
    UNION
    SELECT *
    FROM `USER` `a5`
) `a5`
WHERE ( `a5`.`AGE` > 10 )
```

## 数据库函数

### 聚合函数(使用SqlFn的静态方法调用，或者GroupBy后的方法调用中的参数`IExpSelectGrouping<TGroup, TTables>`中调用)

COUNT(*)

COUNT<T>(T column), 当 column 是(一个二元表达式，并且T的类型是bool) 或者 是一个三元表达式, 会解析成 CASE WHEN 语句

MAX、MIN、AVG、SUM

Join(在分组数据中拼接字符串，不同数据库，调用的函数不同，例如，mysql是group_concat， oracle是listagg等)

### 开窗函数(窗口函数)

RowNumber、Lag、Rank

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

# Ado对象

直接执行sql语句, 可返回`IEnumerable<T>`,`DataTable`,`DataReader`等等

# 待办

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
// 从info表中，按StnId和Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为stn_fzjg，表数据为每个StnId中，按Fzjg数据量进行排序并标记为Index
var stnFzjg = Db.FromTemp(info).GroupBy(a => new { a.StnId, a.Fzjg })
    .OrderByDesc(a => new { a.Group.StnId, i = a.Count() })
    .AsTemp("stn_fzjg", g => new
    {
        g.Group.StnId,
        g.Group.Fzjg,
        Count = g.Count(),
        Index = WinFn.RowNumber().PartitionBy(g.Tables.StnId).OrderByDesc(g.Count()).Value()
    });
// 从info表中，按Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为all_fzjg，表数据为所有Fzjg中，按每个Fzjg的数据量进行排序并标记为Index
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
var allStation = Db.FromTemp(info).GroupBy("ROLLUP(\"StnId\")")
    .AsTemp("all_station", g => new
    {
        StnId = SqlFn.Nvl(g.StnId, "合计"),
        Total = SqlFn.Count()
    });
/**
1. 从stn_fzjg中，筛选出所有前3的Fzjg数量，然后按StnId分组，选择StnId，组内第一Fzjg作为FirstFzjg，组内第一的Count作为FirstCount
2. 从all_fzjg中，筛选出所有前3的Fzjg数量，选择'合计'作为StnId，第一Fzjg作为FirstFzjg，第一的Count作为FirstCount
3. 将1和2的结果Union ALl
4. 转为子查询，inner join all_station
5. select结果列
 */
var result = Db.FromTemp(stnFzjg).Where(t => t.Index < 4)
    .GroupBy(t => new { t.StnId })
    .AsTable(g => new
    {
        StnId = g.Group.StnId!,
        FirstFzjg = g.Join(g.Tables.Index == 1 ? g.Tables.Fzjg.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value(),
        FirstCount = g.Join(g.Tables.Index == 1 ? g.Tables.Count.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value()
    }).UnionAll(Db.FromTemp(allFzjg).Where(t => t.Index < 4).AsTable(g => new
    {
        StnId = "合计",
        FirstFzjg = SqlFn.Join(g.Index == 1 ? g.Fzjg.ToString() : "").Separator("").OrderBy(g.StnId).Value(),
        FirstCount = SqlFn.Join(g.Index == 1 ? g.Count.ToString() : "").Separator("").OrderBy(g.StnId).Value()
    })).AsSubQuery()
    .InnerJoin(allStation, (t, a) => t.StnId == a.StnId)
    .ToSql((t, a) => new
    {
        Jczmc = SqlFn.Nvl(t.StnId,"TT"),
        a.Total,
        t
    });
Console.WriteLine(result);
```

```sql
WITH info AS (
    SELECT SUBSTR("r0"."Plate",1,2) AS "Fzjg", "r0"."StnId"
    FROM "Jobs" "r0"
)
,stn_fzjg AS (
    SELECT "t1"."StnId", "t1"."Fzjg", COUNT(*) AS "Count", ROW_NUMBER() OVER( PARTITION BY "t1"."StnId" ORDER BY COUNT(*) DESC ) AS "Index"
    FROM info "t1"
    GROUP BY "t1"."StnId", "t1"."Fzjg"
    ORDER BY "t1"."StnId", COUNT(*) DESC
)
,all_fzjg AS (
    SELECT '合计' AS "StnId", "t1"."Fzjg", COUNT(*) AS "Count", ROW_NUMBER() OVER( ORDER BY COUNT(*) DESC ) AS "Index"
    FROM info "t1"
    GROUP BY "t1"."Fzjg"
    ORDER BY COUNT(*) DESC
)
,all_station AS (
    SELECT NVL("t1"."StnId",'合计') AS "StnId", COUNT(*) AS "Total"
    FROM info "t1"
    GROUP BY ROLLUP(StnId)
)
SELECT NVL("t4"."StnId",'TT') AS "Jczmc", "t3"."Total", "t4".*
FROM (
    SELECT "t2"."StnId", LISTAGG( CASE WHEN ("t2"."Index" = 1) THEN CAST("t2"."Fzjg" AS VARCHAR(255)) ELSE '' END, '') WITHIN GROUP (ORDER BY "t2"."StnId" ASC) AS "FirstFzjg", LISTAGG( CASE WHEN ("t2"."Index" = 1) THEN CAST("t2"."Count" AS VARCHAR(255)) ELSE '' END, '') WITHIN GROUP (ORDER BY "t2"."StnId" ASC) AS "FirstCount"
    FROM stn_fzjg "t2"
    WHERE ("t2"."Index" < 4)
    GROUP BY "t2"."StnId"
    UNION ALL
    SELECT '合计' AS "StnId", LISTAGG( CASE WHEN ("t2"."Index" = 1) THEN CAST("t2"."Fzjg" AS VARCHAR(255)) ELSE '' END, '') WITHIN GROUP (ORDER BY "t2"."StnId" ASC) AS "FirstFzjg", LISTAGG( CASE WHEN ("t2"."Index" = 1) THEN CAST("t2"."Count" AS VARCHAR(255)) ELSE '' END, '') WITHIN GROUP (ORDER BY "t2"."StnId" ASC) AS "FirstCount"
    FROM all_fzjg "t2"
    WHERE ("t2"."Index" < 4)
) "t4"
INNER JOIN all_station "t3" ON ("t4"."StnId" = "t3"."StnId")
```

