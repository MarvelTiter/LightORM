## How to use?

### configuration
```
var path = Path.GetFullPath("../../../test.db");
ExpSqlFactory.Configuration(option =>
{
    option.UseMySql(connectionString);
});

// get IExpressionContext instance
IExpressionContext context = ExpSqlFactory.GetContext();

```

### inject service in dot net core
```
var path = Path.GetFullPath("../../../test.db");
builder.Services.AddLightOrm(option =>
{
    option.UseMySql(connectionString);
});
```