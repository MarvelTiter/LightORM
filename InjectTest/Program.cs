using MDbContext;
using MDbContext.ExpressionSql.Interface;
using MDbEntity.Attributes;
using Microsoft.Data.Sqlite;

[Table(Name = "t_user")]
class User
{
    [Column(AutoIncrement = true, PrimaryKey = true)]
    public int Id { get; set; }
    [Column(NotNull = true)]
    public string Name { get; set; }
}
internal class DbContext : ExpressionContext
{
    public override void Initialized(IDbInitial db)
    {
        db.CreateTable<User>();
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddLightOrm(option =>
        {
            option.SetDatabase(DbBaseType.Sqlite, () =>
            {
                return new SqliteConnection("DataSource=DB01.db");
            }).InitializedContext<DbContext>();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}