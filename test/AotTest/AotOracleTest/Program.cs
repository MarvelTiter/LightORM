using AotOracleTest;
using LightORM;
using LightORM.Providers.Oracle.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System.Text.Json;

var serviceCollection = new ServiceCollection();
serviceCollection.AddLightOrm(option =>
{
    option.UseOracle("User Id=lightorm_test;Password=lightorm_test;Data Source=localhost:1521/XE;");
    option.SetTableContext<TableContext>();
    option.UseInterceptor<SqlTrace>();
});
var services = serviceCollection.BuildServiceProvider();
var context = services.GetRequiredService<IExpressionContext>();

var users = await context.Select<User>().Include(u => u.Profile).ToListAsync();
var json = JsonSerializer.Serialize(users, JsonContext.Default.IListUser);
Console.WriteLine(json);
Console.ReadKey();
