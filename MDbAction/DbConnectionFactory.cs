using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.SqlClient;

namespace MDbAction
{
    internal class DbConnectionFactory
    {
        public static IDbConnection CreateConnect(int dBType)
        {
            IDbConnection conn = null;
            switch (dBType)
            {
                case 0:
                    conn = new SqlConnection();
                    break;
                case 1:
                    conn = new OracleConnection();
                    break;
                case 2:
                    conn = new MySqlConnection();
                    break;
                default:
                    break;
            }
            return conn;
        }
    }

    public enum DBType
    {
        SqlServer = 0,
        Oracle = 1,
        MySql = 2
    }
}
