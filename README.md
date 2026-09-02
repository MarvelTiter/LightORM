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

## 🏗 架构总览

### 解决方案分层

```
LightORM.slnx
├── src/                        # 生产代码
│   ├── LightORM/               # 核心库 (Assembly: MT.LightORM)
│   ├── LightOrmExtensionGenerator/        # Roslyn 源生成器①（扩展方法生成）
│   ├── LightOrmTableContextGenerator/     # Roslyn 源生成器②（表上下文生成）
│   ├── LightORM.Extensions.DependencyInjection/  # 依赖注入扩展包
│   ├── GenShared/              # 生成器共享代码 (shproj)
│   └── Providers/              # 数据库适配层（7 个 Provider）
│       ├── LightORM.Providers.Sqlite
│       ├── LightORM.Providers.MySql
│       ├── LightORM.Providers.SqlServer
│       ├── LightORM.Providers.Oracle
│       ├── LightORM.Providers.PostgreSQL
│       ├── LightORM.Providers.Dameng      # 达梦（国产数据库）
│       └── LightORM.Providers.KingbaseES  # 人大金仓（国产数据库）
├── test/                       # 测试工程（10 个）
├── DatabaseUtils/              # 独立的数据库工具项目
└── doc/                        # 文档（版本日志）
```

### 核心库内部结构（`src/LightORM`）

| 目录 | 职责 |
|------|------|
| `Interfaces/` | 全部对外接口契约（含 `ExpSql/` 子目录的流式查询接口） |
| `Implements/` | 核心实现（Provider 基类、SQL 方法解析器、表达式信息提供器等） |
| `ExpressionSql/` | 表达式→SQL 的核心引擎（`ExpressionCoreSql`、上下文、事务、作用域） |
| `Builder/` | 各语句构造器（`SelectBuilder` / `InsertBuilder` / `UpdateBuilder` / `DeleteBuilder` / `SqlBuilder`） |
| `Providers/` | 核心库内的 SQL 片段生成 Provider（Select/Insert/Update/Delete/Group/Include） |
| `SqlExecutor/` | SQL 执行层（`SqlAdo`、原始 SQL 执行、表达式构建委托映射、连接管理） |
| `Attributes/` | 实体映射特性（`LightColumn`、`LightFlat` 等） |
| `Cache/` | 表达式解析缓存、对象池 |
| `Repository/` | 仓储模式（`ILightOrmRepository<T>`） |
| `DbStruct/` | 数据库结构读取（表结构、建表） |
| `DbEntity/` | 实体模型 |
| `Performances/` | 性能相关（`StringBuilderPool` 等） |
| `Models/` | 上下文模型（`JsonColumnContext`、`UpsertContext` 等） |
| `Utils/` | 工具类与访问器（`Vistors/` 表达式访问器） |

### 核心设计理念

1. **零依赖** —— 核心库 `MT.LightORM` 只依赖 .NET 标准库，纯 `System.Linq.Expressions` + `System.Data.Common` 实现。
2. **表达式树驱动** —— 一切 API 以 `Expression<Func<...>>` 为入口，编译时类型安全，运行时动态构建 SQL。
3. **源生成器替代反射** —— 通过 Roslyn Source Generator 静态收集实体元数据，避免运行时反射开销，支持 **AOT 编译**。
4. **适配器模式** —— `IDatabaseAdapter` 抽象了不同数据库的方言差异，新增数据库只需实现一个适配器。
5. **性能优先** —— 表达式编译缓存、对象池、`StringBuilderPool`，BenchmarkDotNet 持续压测。

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
| KingbaseES | `LightORM.Providers.KingbaseES` | [![NuGet](https://img.shields.io/nuget/v/LightORM.Providers.KingbaseES.svg)](https://www.nuget.org/packages/LightORM.Providers.KingbaseES) |

> 批量插入（BulkCopy）支持情况：SqlServer / MySql / Oracle / Dameng 支持，PostgreSQL / SQLite 不支持。

### 目标框架与 AOT

- 核心库 `MT.LightORM`：`net462` / `netstandard2.0` / `net8.0` / `net10.0`
- **`IsAotCompatible = true`** —— 全链路 AOT 裁剪支持（泛型参数改造、子查询用 Builder 重组、插值字符串常量提取等）
- 生成器针对 `netstandard2.0` 打包进 NuGet 的 `analyzers/dotnet/cs` 目录

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

## 🔬 核心引擎实现

本节深入剖析「表达式树 → SQL」这条主线的真实实现机制。

### 一条数据流

整个 ORM 的本质是把 `Expression<Func<...>>` 变成 SQL 字符串 + 参数字典：

```
Expression<Func<T,bool>>  (用户写的 Lambda)
        │
        ▼
ExpressionResolver          # 递归遍历表达式树，边遍历边追加 SQL 片段
        │   (Utils/ExpressionResolver.cs)
        ▼
ResolveContext              # 维护「参数表达式 → 表/别名」映射，支持父级上下文回溯
        │   (Utils/ResolveContext.cs)
        ▼
ISqlMethodResolver          # 把 C# 方法调用映射为 SQL 函数/语法
        │   (BaseSqlMethodResolver.cs 基类 + 各 Provider 重写)
        ▼
SelectBuilder / SqlBuilder  # 收集各片段，按方言组装成完整 SQL
        │   (Builder/*.cs)
        ▼
IDatabaseAdapter            # 方言适配：标识符引用、分页、布尔/日期字面量、JSON 函数
        │
        ▼
SQL 字符串 + DbParameters
```

**关键设计**：解析器（ExpressionResolver）只负责「片段生成」，Builder 负责「结构组装」，Adapter 负责「方言翻译」。三层职责分离，这是它能支持 7 个数据库且代码不臃肿的根本原因。

### ExpressionResolver：表达式树的翻译器

`Visit(Expression)` 用 switch 模式匹配分派到 11 种节点类型：

```csharp
public Expression? Visit(Expression? expression) => expression switch
{
    LambdaExpression     => Visit(VisitLambda(...)),
    BinaryExpression     => Visit(VisitBinary(...)),
    ConditionalExpression=> Visit(VisitConditional(...)),   // → CASE WHEN
    MethodCallExpression => Visit(VisitMethodCall(...)),    // → 函数/索引器
    NewArrayExpression   => Visit(VisitNewArray(...)),      // → IN (...)
    NewExpression        => Visit(VisitNew(...)),           // → SELECT 投影 + AS 别名
    UnaryExpression      => Visit(VisitUnary(...)),         // → NOT / Convert
    ParameterExpression  => Visit(VisitParameter(...)),     // → 表别名
    MemberInitExpression => Visit(VisitMemberInit(...)),
    MemberExpression     => Visit(VisitMember(...)),        // → 列名
    ConstantExpression   => Visit(VisitConstant(...)),      // → 字面量/参数
    _ => null
};
```

**几个精妙细节**：

- **三元表达式 → CASE WHEN**：`a ? b : c` 直接生成 `CASE WHEN a THEN b ELSE c END`。
- **索引器双重语义**：`int` 索引 → 数组下标（JSON 数组 `[3]`）；`string` 索引 → 属性访问（JSON 对象 `["prop1"]`）。
- **`IsNot` 标志位**：遇到 `Not` 节点置位，后续 `Contains`/`In` 等方法解析时消费，生成 `NOT LIKE`/`NOT IN`。
- **列名 vs 值**：`MemberExpression.Expression` 是 `ParameterExpression` → 列；是 `ConstantExpression`（闭包变量）→ 值。

### ResolveContext：作用域与别名管理

核心是 `parent` 引用 + `Depth` 层级。解析子查询时创建 `new ResolveContext(upperContext, ...)`，当子查询引用**外层列**（如 EXISTS 里 `ur.UserId == u.UserId`），`GetTable` 沿 `parent` 链向上回溯查找。

```csharp
public TableInfo GetTable(ParameterExpression pExp)
{
    if (lambdaParameterInfos.TryGetValue(key, out var ti)) return ti;
    if (parent is not null) return parent.GetTable(pExp);  // 向上回溯
    throw new LightOrmException("解析ParameterExpression出错");
}
```

### ISqlMethodResolver：C# 方法 → SQL 翻译表

基类 `BaseSqlMethodResolver` 定义一套 `virtual` 方法，覆盖所有可翻译的方法语义：

| C# 写法 | SQL 翻译 |
|---------|---------|
| `Count()` / `Count(predicate)` | `COUNT(*)` / `COUNT(CASE WHEN ... THEN 1 ELSE NULL END)` |
| `Sum/Max/Min/Avg(predicate, selector)` | 自动展开成 `CASE WHEN` 条件聚合 |
| `Contains` / `StartsWith` / `EndsWith` | `LIKE '%...%'`（各库拼接方式不同） |
| `Abs/Round/Nvl/IsNull/NullThen/Coalesce` | 对应 SQL 函数 |
| `RowNumber/Lag/Rank` + `PartitionBy/OrderBy/Value` | 窗口函数链式拼接 |
| `Case/When/Then/Else/End` | `CASE WHEN ... THEN ... END` |
| `Join/Separator/OrderBy/Value` | 分组内字符串聚合（`STRING_AGG`/`LISTAGG`/`GROUP_CONCAT`） |

**关键点**：基类里大量方法直接 `throw new NotSupportedException()`（如 `StartsWith`、`Contains`、`ToString`、`JsonQuery`），强制各 Provider 重写——因为字符串拼接、类型转换、JSON 函数在不同数据库语法差异巨大。

**子查询的优雅处理**：当方法返回类型是 `IExpSelect` 时，识别为子查询，**递归复用整个 SelectBuilder 管线**而非表达式编译：

```csharp
public virtual void Exits(IExpressionResolver resolver, MethodCallExpression methodCall)
{
    var builder = methodCall.CreateSelectBuilder();   // 递归创建新的 SelectBuilder
    builder.SetResolveParentContext(resolver.Context); // 传递父上下文（支持引用外层列）
    builder.IsSubQuery = true;
    resolver.Sql.AppendLine(resolver.IsNot ? "NOT EXISTS (" : "EXISTS (");
    builder.Build(resolver.Sql, resolver.Context.Database, resolver.Level + 1);
    resolver.Sql.Append(')');
}
```

这也是 AOT 改造的关键——子查询不用表达式编译，而是复用 Builder 管线。

#### Provider 重写示例（SqlServer）

`SqlServerMethodResolver` 重写了 `ToString`、`StartsWith`、`Contains`、`Substring`、`Trim`、`Value`、`JsonQuery` 等：

- `ToString()` → `CONVERT(VARCHAR(MAX), ...)`，且**把 .NET 日期格式字符串映射成 SQL Server 的 style code**（`"yyyy-MM-dd"` → 23，`"yyyyMMdd"` → 112 等），这是非常贴合实际业务痛点的细节。
- `Contains` 区分 `string.Contains`（→ `LIKE '%'+x+'%'`）和集合 `Contains`（→ `IN (...)`）。
- `Join` 聚合根据 `SqlServerVersion` 决定用 `STRING_AGG`（2017+）还是抛异常。
- `JsonQuery` → `JSON_VALUE`，`JsonSet` → `JSON_MODIFY`。

**版本感知**：`SqlServerMethodResolver` 构造函数接收 `SqlServerVersion` 枚举，同一 SQL 片段在不同 SQL Server 版本下生成不同函数——这是 `TRIM`（2017+ 原生 vs 兼容写法）等函数能正确工作的原因。

### SelectBuilder：SQL 的结构组装器

#### 片段收集模型

`ResolveExpressions()` 遍历所有已注册的表达式，调用 `Expression.Resolve()` 得到**片段字符串**，然后按 `SqlPartial` 类型分发到不同容器：

```csharp
// HandleResult 的核心分发逻辑
SqlPartial.Where    → Where.Add(sql)      // 最后用 AND 连接
SqlPartial.Join     → joinInfo.Where = sql
SqlPartial.Select   → SelectValue = sql
SqlPartial.GroupBy  → GroupBy = sql
SqlPartial.OrderBy  → OrderBy.Add(sql)
SqlPartial.Having   → Having.Add(sql)
```

#### 递归组装（`Build`）

`Build` 方法按固定顺序拼接完整 SQL，且**递归处理子查询/CTE/Join 子查询/Union**，每递归一层 `currentLevel+1` 控制缩进和表别名层级（`a0/a1/a2`）：

```
[WITH cte AS (...)]
SELECT [DISTINCT] {SelectValue}
FROM {table | (subquery) alias}
[JOIN ...]
[WHERE ...]
[GROUP BY [ROLLUP|CUBE|GROUPING SETS]]
[HAVING ...]
[ORDER BY ...]
[paging]
[UNION ...]
```

#### 导航属性自动 JOIN（`ScanNavigate`）

当检测到表达式引用了导航属性（`result.UseNavigate`），会递归 `ScanNavigate`：
- 一对多关系 → 中间表（mapping table）需要 **2 次 JOIN**（主表→中间表→目标表）
- 一对一关系 → 1 次 JOIN
- 嵌套导航 → 递归下降，`NavigateDeep` 递减

这是 `Include`/`Any` 自动生成 JOIN 的底层实现。

#### 预估 SQL 长度（`EstimateSqlLength`）

`ToSqlString` 先 `EstimateSqlLength()` 预估字符串长度，再 `StringBuilderPool.Get(out sql, estimatedSize)` 租一个**预分配好容量**的 StringBuilder。这避免了 StringBuilder 反复扩容的数组拷贝，配合 `StringBuilderPool`（版本日志 v2026.08.21 新增）显著降低 GC 压力。

### 表达式缓存：FNV-1a 哈希 + 对象池

```csharp
private readonly record struct CacheKey(SqlAction SqlAction, ulong Hash);
private static readonly ConcurrentDictionary<CacheKey, ExpressionResolvedResult> cache = new();
```

`ExpressionHasher` 用 **FNV-1a 64 位哈希**遍历表达式树，把节点类型、成员名、类型元数据令牌喂进哈希。关键设计：
- **结构相同 → 哈希相同**（命中缓存）
- **常量值不参与哈希**（`x.Age > 10` 和 `x.Age > 20` 共享缓存项）
- 命中后若标记 `NeedToExtractValues`，单独调用 `ExpressionValueExtract.Extract()` 重新提取本次参数值

这是 benchmark 里「开启缓存后 CTE 查询 135.6us → 89.5us」的原理：**结构解析只做一次，变量值提取每次都做，两者分离**。

### 参数化与值提取

闭包变量值的读取被编译成 getter 委托并缓存（key 是 `类型名_成员链名`），避免反射：

```csharp
private static readonly ConcurrentDictionary<string, Func<object?, object?>> getterCache = [];
// 用 "类型FullName_成员链名" 作 key，编译 Expression.Lambda 得到 getter
```

`VisitConstant` 中值不直接内联，而是生成占位符存 `DbParameters`，最终 `HandleSqlParameters` 统一替换为真实参数引用。特殊类型处理：
- `null` → 占位符替换为 `IS NULL`
- `bool` → 数据库方言布尔字面量（如 SqlServer 是 `1/0`）
- 集合 → 展开成 `@p_0, @p_1, ...`（用于 `IN`）
- `DateTime` → 方言日期格式

### 三种方言适配层次

| 层次 | 文件 | 职责 |
|------|------|------|
| **适配器** | `IDatabaseAdapter` + 各 Provider 的 `CustomXxxAdapter` | 标识符引用符号、分页、布尔/日期字面量、批量操作模板 |
| **方法解析器** | `BaseSqlMethodResolver` + 各 Provider 的 `XxxMethodResolver` | 函数名映射、`LIKE` 拼接、JSON 函数 |
| **表处理器** | `XxxTableHandler` | 建表语句、批量写入的具体实现（如 SqlServer 的 BulkCopy） |

新增一个数据库 = 实现这 3 个类，这是整个架构的可扩展性核心。

## 🧩 关键 API 入口

```csharp
// 全局配置（非 DI 场景）
ExpSqlFactory.Configuration(option => {
    option.UseSqlite("DataSource=test.db")
          .SetTableContext(new TestTableContext())
          .SetWatcher(aop => aop.DbLog = (sql, p) => Console.WriteLine(sql));
});
IExpressionContext Db = ExpSqlFactory.GetContext();

// DI 场景
services.AddLightOrm(option => option.UseMySql(connStr));
```

**核心接口层级：**

```
IExpressionContext          # 顶层上下文（多库、事务、Union、FromQuery/FromTemp）
├── IScopedExpressionContext   # 单元操作（多库事务，Begin/Commit/Rollback + 异步版本）
├── ISingleScopedExpressionContext # 单库单元操作
└── ITransientContext          # SwitchDatabase 后的临时上下文

IExpSelect<T> / IExpInsert<T> / IExpUpdate<T> / IExpDelete<T>   # 流式语句构建
IExpSelectGroup<TGroup, TTable>   # 分组后的聚合/窗口函数上下文
IExpTemp<T>                        # CTE 临时表
```

## 🤝 参与贡献

欢迎提交 Issue 和 PR！

## 📄 开源协议

[MIT License](LICENSE)