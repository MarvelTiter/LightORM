using MDbEntity.Attributes;
using System.ComponentModel.DataAnnotations;

namespace LightORM.Test2
{
    public enum AccountStatus : byte
    {
        [Display(Name = "正常")]
        Normal = 1,
        [Display(Name = "鎖定")]
        Lock = 0,
    }
    [Table(Name = "user")]
    public class MacaoUser 
    {
        [Column(Name = "userId", PrimaryKey = true)]
        public string UserId { get; set; }
        [Column(Name = "userName")]
        public string UserName { get; set; }
        [Column(Name = "password")]
        public string Password { get; set; }
        [Column(Name = "advancedPassword")]
        public string AdvancedPassword { get; set; }
        [Column(Name = "status")]
        public AccountStatus Status { get; set; }
        [Column(Name = "modificationTime")]
        public DateTime ModificationTime { get; set; }
        [Column(Name = "createTime")]
        public DateTime CreateTime { get; set; }

        [Column(Name = "lastLoginTime")]
        public DateTime? LastLogin { get; set; }
    }

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

        [TestMethod]
        public void UpdateSet()
        {
            Watch(db =>
            {
                 db.Update<MacaoUser>()
                                    .Set(u => u.LastLogin, DateTime.Now)
                                    .Where(u => u.UserId == "admin").Execute();
            });
        }
    }
}