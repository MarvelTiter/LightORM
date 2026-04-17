## How to use?

### configuration
```
ExpSqlFactory.Configuration(option =>
{
    option.UseKingbaseES(connectionString);
});

// get IExpressionContext instance
IExpressionContext context = ExpSqlFactory.GetContext();

```

### inject service in dot net core
```
builder.Services.AddLightOrm(option =>
{
    option.UseKingbaseES(connectionString);
});
```