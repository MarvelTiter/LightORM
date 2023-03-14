using LightORM.Test2.Models;

namespace LightORM.Test2
{
	[TestClass]
	public class CancelTokenTest : TestBase
    {
		[TestMethod]
		public void Test1()
		{
			Watch(db =>
			{
				CancellationTokenSource source = new CancellationTokenSource();
				db.Select<Users>()
				.AttachCancellationToken(source.Token)
				.ToListAsync();
				db.Select<Users>()
				.ToListAsync();
			});
		}
    }
}