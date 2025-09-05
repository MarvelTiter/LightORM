## How to use?

### configuration
```
ExpSqlFactory.Configuration(option =>
{
    option.UseDameng(connectionString);
});

// get IExpressionContext instance
IExpressionContext context = ExpSqlFactory.GetContext();

```

### inject service in dot net core
```
builder.Services.AddLightOrm(option =>
{
    option.UseDameng(connectionString);
});
```