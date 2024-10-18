using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    [TestClass]
    public class Other
    {
        [TestMethod]
        public void M()
        {
            StringBuilder sb = new();
            sb.Append("Hello World!");
            sb.Insert(5, " Insert");
        }

        private DbConnection? connection;

        private DbConnection GetConnection()
        {
            if (Interlocked.CompareExchange(ref connection, connection, null) == null)
            {
                connection = new SQLiteConnection();
            }
            return connection!;
        }

        [TestMethod]
        public void Get()
        {
            var c = GetConnection();
            var c2 = GetConnection();
        }
    }
}
