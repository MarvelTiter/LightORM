using MDbEntity.Attributes;

namespace LightORM.Test2
{
    [TestClass]
    public class UpdateTest : TestBase
    {

        public class Model1
        {
            [Column(PrimaryKey = true)]
            public int Id { get; set; }
            [Column(PrimaryKey = true)]
            public string? Name { get; set; }
        }

        [TestMethod]
        public void UpdateTest01()
        {
            Watch(db =>
            {
                var u = new Model1();
                u.Id = 1;
                u.Name = "test";
                var sql = db.Update<Model1>().AppendData(u).ToSql();
                Console.WriteLine(sql);
            });
        }
    }
}