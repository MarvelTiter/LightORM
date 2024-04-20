# DbContext 新版本用法

## 该库本质是使用`IDbConnection`对象执行 Sql 语句，`IExpressionContext`的调用，尽量还原原生 sql 的写法逻辑。同时，查询返回实体的时候，列名需要与实体属性/字段名称匹配（或者`LightColumnAttribute`匹配）。

## 直接使用。获取`IExpressionContext`对象

```csharp
var path = Path.GetFullPath("../../../test.db");
ExpSqlFactory.Configuration(option =>
{
    option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
    option.SetWatcher(aop =>
    {
        aop.DbLog = (sql, p) =>
        {
            Console.WriteLine(sql);
        };
    }).InitializedContext<TestInitContext>();
});

// get IExpressionContext instance
IExpressionContext context = ExpSqlFactory.GetContext();
```
## 容器中使用。依赖注入
```csharp
var path = Path.GetFullPath("../../../test.db");
builder.Services.AddLightOrm(option =>
{
    option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance).InitializedContext<TestInitContext>();
});
```
## 查询

```csharp
// 普通查询
context.Select<T>().Where(Expression<Func<T, bool>>).ToList();
// 指定列
context.Select<T>(t => new {  }).Where(Expression<Func<T, bool>>).ToList();
// Max, Count, Avg, Sum 
context.Select<T>().Where(Expression<Func<T, bool>>).Sum();
// 分页
context.Select<T>().Count(out long total).Where(Expression<Func<T, bool>>).Paging(index, size).ToList();
...
```
## 插入

```csharp
// 插入实体
context.Insert<T>(entity).Execute();
// 忽略指定列
context.Insert<T>(entity).IgnoreColumns(t => new {  }).Execute();
// 批量插入 entities 为集合类型
context.Insert<T>(entities).Execute();
```

## 更新

```csharp
// 根据主键更新实体。配置了主键列，where可选
context.Update<T>(entity).Execute();
// 实体更新，更新指定列。
context.Update<T>(entity).UpdateColumns(t => new {}).Execute();
// 一般更新，更新指定列
context.Update<T>().UpdateColumns(() => new {}).Where(Expression<Func<T, bool>>).Execute();
context.Update<T>().Set<TProp>(t => t.Prop, value).Where(Expression<Func<T, bool>>).Execute();
// 忽略指定列
context.Update<T>(entity).IgnoreColumns(t => new {}).Execute();
// 批量更新 entities 为集合类型
context.Insert<T>(entities).Execute();
```

## ADO 对象

```csharp
context.Ado
```
