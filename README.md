# DbContext 新版本用法

## 该库本质是使用`IDbConnection`对象执行 Sql 语句，`IExpressionContext`的调用，尽量还原原生 sql 的写法逻辑。同时，查询返回实体的时候，列名需要与实体属性/字段名称匹配（或者`ColumnAttribute`匹配）。

## 创建 IExpressionContext 对象

```csharp
var option = new ExpressionSqlOptions();
option.SetDatabase(DbBaseType.Sqlite, Func<IDbConnection>);
static IExpressionContext db = new ExpressionSqlBuilder(option)
                .Build();
```

## 使用事务

```csharp
private void Watch(Action<IExpressionContext> action)
{
   var option = new ExpressionSqlOptions().SetDatabase(DbBaseType.Sqlite, SqliteDbContext)
   	.SetSalveDatabase("Mysql", DbBaseType.MySql, () => new SqliteConnection())
   	.SetWatcher(option =>
   	{
   		option.BeforeExecute = e =>
   		{
   			Console.Write(DateTime.Now);
   			Console.WriteLine(" Sql => \n" + e.Sql + "\n");
   		};
   	});

   IExpressionContext eSql = new ExpressionSqlBuilder(option).Build();
   Stopwatch stopwatch = new Stopwatch();
   stopwatch.Start();
   action(eSql);
   stopwatch.Stop();
   Console.WriteLine($"Cost {stopwatch.ElapsedMilliseconds} ms");
   Console.WriteLine(eSql);
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
        db.BeginTransAsync();
        db.Update<User>().UpdateColumns(() => new { u.UserName }).Where(u => u.UserId == "admin").Execute();
        db.Insert<User>(u).Execute()
        db.CommitTransaction();
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

## 查询 Count、Max 等

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
        db.Insert<User>(u).ExecuteAsync();
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
        db.Update<Users>(u).IgnoreColumns(u => new { u.Tel }).Where(u => u.Age == 10).ExecuteAsync();
    });
}
```

## ADO 对象

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
