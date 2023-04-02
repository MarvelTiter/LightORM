## How to use?

### create an singleton instance of `IExpressionSqlContext` like this
```
var option = new ExpressionSqlOptions();
var builder = new ExpressionSqlBuilder(option);
IExpressionSqlContext context = builder.Build(provider);
```

### inject service in dot net core
```
builder.Services.AddLightOrm(option =>
{
    option.SetDatabase(DbBaseType.Sqlite, () =>
    {
        return new SqliteConnection("DataSource=DB01.db");
    }).InitializedContext<DbContext>();
});
```