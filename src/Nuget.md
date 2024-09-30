## How to use?

### configuration
```
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

### inject service in dot net core
```
var path = Path.GetFullPath("../../../test.db");
builder.Services.AddLightOrm(option =>
{
    option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance).InitializedContext<TestInitContext>();
});
```