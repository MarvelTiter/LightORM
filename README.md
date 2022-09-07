# DbContext 新版本用法
## 该库本质是使用`IDbConnection`对象执行Sql语句，`IExpressionContext`的调用，尽量还原原生sql的写法逻辑。同时，查询返回实体的时候，列名需要与实体属性/字段名称匹配（或者`ColumnAttribute`匹配）。
## 创建IExpressionContext对象
``` csharp
static IExpressionContext db = new ExpressionSqlBuilder()
                .SetDatabase(DbBaseType.Sqlite, Func<IDbConnection>)
                .Build();
```
## 使用事务
```csharp
private void Watch(Action<IExpressionContext> action)
{
    IExpressionContext db = new ExpressionSqlBuilder()
        .SetDatabase(DbBaseType.Sqlite, SqliteDbContext)
        .SetWatcher(option =>
        {
            option.BeforeExecute = e =>
            {
                Console.Write(DateTime.Now);
                Console.WriteLine(" Sql => \n" + e.Sql + "\n");
            };
        })
        .Build();
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
    action(db);
    stopwatch.Stop();
    Console.WriteLine($"Cost {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine("====================================");
}
public void TransactionTest()
{
    Watch(db =>
    {
        var u = new User();
        u.UserId = "User002";
        u.UserName = "测试001";
        u.Password = "0000";
        // 返回独立的 IExpressionContext 对象
        var trans = db.BeginTransaction();
        trans.Update<User>().UpdateColumns(() => new { u.UserName }).Where(u => u.UserId == "admin").AttachTransaction();
        trans.Insert<User>().AppendData(u).AttachTransaction();
        trans.CommitTransaction();
    });
}
```
## 查询
```csharp
public void V2Select()
{
    Watch(db =>
    {
        db.Select<Power, RolePower, UserRole>()
        .InnerJoin<RolePower>(w => w.Tb1.PowerId == w.Tb2.PowerId)
        .InnerJoin<UserRole>(w => w.Tb2.RoleId == w.Tb3.RoleId)
        .Where(w => w.Tb3.UserId == "admin")
        .ToList();
    });
}
```
## 查询 Count、Max等
```csharp
public void V2SelectFunc()
{
    Watch(db =>
    {
        var s = "sss";
        db.Select<Users>(w => new
        {
            UM = SqlFn.Count(() => w.Age > 10),
            UM2 = SqlFn.Count(() => w.Duty == s),
        }).ToDynamicList();
    });
}
```
## 插入
```csharp
public void V2Insert()
{
    Watch(db =>
    {
        var u = new User();
        u.UserId = "User001";
        u.UserName = "测试001";
        u.Password = "0000";
        db.Insert<User>().AppendData(u).ExecuteAsync();
    });
}
```
## 更新
```csharp
public void V2Update()
{
    Watch(db =>
    {
        var u = new Users();
        db.Update<Users>().AppendData(u).IgnoreColumns(u => new { u.Tel }).Where(u => u.Age == 10).ExecuteAsync();
    });
}
```
## ADO对象
```csharp
public void AdoTest()
{
    Watch(db =>
    {
        var users = db.Ado.Query<User>("select * from user").ToList();
        foreach (var u in users)
        {
            Console.WriteLine($"{u.UserId} - {u.UserName}");
        }
    });
}
```
# DBManage初始版本用法

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


