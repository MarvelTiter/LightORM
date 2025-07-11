## How to use?

### configuration
```
var path = Path.GetFullPath("../../../test.db");
ExpSqlFactory.Configuration(option =>
{
    option.UseSqlite(connectionString);
});

// get IExpressionContext instance
IExpressionContext context = ExpSqlFactory.GetContext();

```

### inject service in dot net core
```
var path = Path.GetFullPath("../../../test.db");
builder.Services.AddLightOrm(option =>
{
    option.UseSqlite(connectionString);
});
```