global using LightORM;
using TestWeb.Components;
using LightORM.Providers.Sqlite.Extensions;
using LightORM.Providers.Oracle.Extensions;
using TestWeb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLightOrm(option =>
{
    var path = Path.GetFullPath("../test.db");
    //option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
    option.UseSqlite("DataSource=" + path);
    option.UseOracle(option =>
    {
        option.DbKey = "Oracle";
        option.MasterConnectionString = "User ID=IFSAPP;Password=IFSAPP;Data Source=RACE;";
    });
    option.SetWatcher(aop =>
    {
        aop.DbLog = (sql, p) =>
        {
            Console.WriteLine(sql);
        };
    });//.InitializedContext<TestInitContext>();
});
builder.Services.AddScoped<Services1>();
builder.Services.AddScoped<Services2>();
//builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
