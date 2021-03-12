using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace MDbAction {
    internal class DbConnectionFactory
    {
        private static Type[] TypeCache = new Type[4];
        public static IDbConnection CreateConnect(int dBType)
        {
            IDbConnection conn = null;
            switch (dBType)
            {
                case 0:
                    conn = new SqlConnection();
                    break;
                case 1:
                    conn = OracleAssembly();
                    break;
                case 2:
                    conn = MySqlAssembly();
                    break;
                case 3:
                    conn = SQLiteAssembly();
                    break;
                default:
                    break;
            }
            return conn;
        }

        private static IDbConnection MySqlAssembly()
        {
            Type t;
            if (TypeCache[2] == null)
            {
                var asm = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "MySql.Data.dll");
                t = asm.GetType("MySql.Data.MySqlClient.MySqlConnection");
                if (t == null) throw new DllNotFoundException("MySql.Data.dll 加载失败");
                TypeCache[2] = t;
            }
            else
                t = TypeCache[2];
            var c = t.GetConstructors()[0];
            return (IDbConnection)c.Invoke(null);
        }

        private static IDbConnection OracleAssembly()
        {
            Type t;
            if (TypeCache[1] == null)
            {
                var asm = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "Oracle.ManagedDataAccess.dll");
                t = asm.GetType("Oracle.ManagedDataAccess.Client.OracleConnection");
                if (t == null) throw new DllNotFoundException("Oracle.ManagedDataAccess.dll 加载失败");
                TypeCache[1] = t;
            }
            else
                t = TypeCache[1];
            var c = t.GetConstructors()[0];
            return (IDbConnection)c.Invoke(null);
        }

        private static IDbConnection SQLiteAssembly()
        {
            Type t;
            if (TypeCache[3] == null)
            {
                var asm = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "Microsoft.Data.Sqlite.dll");//
                t = asm.GetType("Microsoft.Data.Sqlite.SqliteConnection");
                if (t == null) throw new DllNotFoundException("Microsoft.Data.Sqlite.dll 加载失败");
                TypeCache[3] = t;
            }
            else
                t = TypeCache[3];
            var c = t.GetConstructors()[0];
            return (IDbConnection)c.Invoke(null);
        }
    }
}
