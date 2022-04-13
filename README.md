# DBManage

#### 解析表达式树转换 SQL

#### Dapper 执行语句

## 初始化，获取`DbContext`

```csharp
// 初始化数据库类型 0 - SqlServer; 1 - Oracle; 2 - MySql
DbContext.Init(1);

// 根据IDbConnection 获取
using(IDbConnection conn = GetDbConnection()) {
    var db = conn.DbContext();
}
// new
using var db = new DbContext(dbType, IDbConnection);
```

## 构建 Sql

### Select

```csharp
db.DbSet.Select<T>()
  .InnerJoin<T2>((t,t2) => t.XXX == t2.XXX)
  .Where(whereExpression)
  .OrderBy(t => t.XXX)
  .Paging(from, to);

db.DbSet.Select<T,T1,T2>((t, t1, t2) => new {
    AgeCount = Fn.Sum(() => t.Age > limit.Age && t.Age < 15),
    //  =>  SUM(CASE WHEN a.Age > 10 THEN 1 ELSE 0 END) AgeCount
    ClassCount = Fn.Count(() => t.ClassID > 10)
    //  =>  COUNT(CASE WHEN a.ClassID > 10 THEN 1 ELSE null END) ClassCount
})
  .Where(t => whereLambda)
  .GroupBy(t => new { t.XXX, t.XXX });
```

### Insert

```csharp
db.DbSet.Insert<T>(T entity);
```

### Update

```csharp
// 更新实体，忽略指定列 (主键等)
db.DbSet.Update<T>(T entity, t => new { t.IgnoreColumn })
  .Where(t => whereLambda);

// 更新指定列
var entity = obj;//
db.DbSet.Update<T>(() => new { entity.Age, entity.Name })
  .Where(t => whereLambda);
```

### Delete

```csharp
db.DbSet.Delete<T>()
  .Where(t => whereLambda);
```

## 执行 Sql

```csharp
// DbContext上的拓展方法
IEnumerable<T> result = db.DbSet.Query<T>();
T result = db.DbSet.Single<T>();
int result = db.Execute();
DataTable result = db.QueryDataTable();
// ExpressionSqlCore上的扩展方法
IEnumerable<M> result = await db.DbSet.Select<T>().Where(whereExpression).ToListAsync<M>();
M result = await db.DbSet.Select<T>().Where(whereExpression).FirstAsync<M>();
DataTable dt = await db.DbSet.Select<T>().Where(whereExpression).ToDataTableAsync();
```
