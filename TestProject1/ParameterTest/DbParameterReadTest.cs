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
            var dic = DbParameterReader.ReadToDictionary("@Date, @Age, @Name", p1);
            Assert.IsTrue((DateTime)dic["Date"] == p1.Date);
            Assert.IsTrue((int)dic["Age"] == p1.Age);
            Assert.IsTrue((string)dic["Name"] == p1.Name);
        }
    }
}
