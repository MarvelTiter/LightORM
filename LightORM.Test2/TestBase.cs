using MDbContext;
using MDbContext.ExpressionSql;
using MDbContext.ExpressionSql.Interface;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace LightORM.Test2
{
	public class TestModel
	{
        public int Id { get; set; }
		public string Name { get; set; }
    }
    public class IniTest : DbInitialContext
    {
        public override void Initialized(IDbInitial db)
        {
			var datas = Enumerable.Range(1,10).Select(i=>new TestModel { Id = i, Name = $"Name_{i}" }).ToArray();
			db.CreateTable<TestModel>(datas: datas);
        }
    }
    [TestClass]
	public class TestBase
	{
		protected void Watch(Action<IExpressionContext> action)
		{
			var option = new ExpressionSqlOptions().SetDatabase(DbBaseType.Sqlite, SqliteDbContext)
				.SetSalveDatabase("Mysql", DbBaseType.MySql, () => new SqliteConnection())
				.SetWatcher(option =>
				{
					option.BeforeExecute = e =>
					{
						Console.Write(DateTime.Now);
						Console.WriteLine(" Sql => \n" + e.Sql + "\n");
					};
				}).InitializedContext<IniTest>();

			IExpressionContext eSql = new ExpressionSqlBuilder(option).Build();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			action(eSql);
			stopwatch.Stop();
			Console.WriteLine($"Cost {stopwatch.ElapsedMilliseconds} ms");
			Console.WriteLine(eSql);
			Console.WriteLine("====================================");
		}

		private IDbConnection SqliteDbContext()
		{
			DbContext.Init(DbBaseType.Sqlite);
			var path = Path.GetFullPath("../../../Demo.db");
			var conn = new SqliteConnection($"DataSource={path}");
			return conn;
		}
	}
}