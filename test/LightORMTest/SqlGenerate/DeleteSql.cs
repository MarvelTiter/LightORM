namespace LightORMTest.SqlGenerate;

[TestClass]
public class DeleteSql : TestBase
{
    [TestMethod]
    public void D1_Delete_Batch()
    {
        var datas = GetList();

        var sql = Db.Delete(datas.ToArray())
            .Where(s => s.Recive.Code == 100)
            .ToSql();
        Console.WriteLine(sql);
        List<SmsLog> GetList()
        {
            return new List<SmsLog>
            {
                new() ,
                new() ,
                new() ,
                new() ,
                new() ,
                new() ,
            };
        }
    }
}
