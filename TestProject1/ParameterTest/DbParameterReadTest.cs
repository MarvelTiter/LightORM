using LightORM.Cache;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.ParameterTest
{
    [TestClass]
    public class DbParameterReadTest
    {
        [TestMethod]
        public void ReadAnonymousTypeParameter()
        {
            var p1 = new
            {
                Date = DateTime.Now,
                Age = 18,
                Name = "Marvel"
            };
            var type = p1.GetType();
            var reader = DbParameterReader.CreateReader("@Date, @Age, @Name", type);
            var cmd = new SQLiteCommand();
            reader.Invoke(cmd, p1);
        }
    }
}
