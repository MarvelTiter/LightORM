## How to use?

### inject service in dot net core
```
builder.Services.AddLightOrm(option =>
{
    // LightORM.Providers.Sqlite
    option.UseSqlite(connectionString);
    // LightORM.Providers.Oracle
    option.UseOracle(connectionString);
    // LightORM.Providers.MySql
    option.UseMySql(connectionString);
    // LightORM.Providers.SqlServer
    option.UseSqlServer(version, connectionString);
    // LightORM.Providers.PostgreSQL
    option.UsePostgreSQL(connectionString);
});
```

### configuration
```
ExpSqlFactory.Configuration(option =>
{
    // LightORM.Providers.Sqlite
    option.UseSqlite(connectionString);
    // LightORM.Providers.Oracle
    option.UseOracle(connectionString);
    // LightORM.Providers.MySql
    option.UseMySql(connectionString);
    // LightORM.Providers.SqlServer
    option.UseSqlServer(version, connectionString);
    // LightORM.Providers.PostgreSQL
    option.UsePostgreSQL(connectionString);
});

// get IExpressionContext instance
IExpressionContext context = ExpSqlFactory.GetContext();

```