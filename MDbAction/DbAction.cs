using Dapper;
using MDbAction.IServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MDbAction
{
    /// <summary>
    /// 1. 查询
    /// 2. 执行Sql，返回受影响函数
    /// 3. 执行Sql，返回DataTable
    /// 4. 执行Sql，返回IEnumerable<T>
    /// </summary>
    public class DbAction : IDbAction
    {
        private readonly int dbType;
        private readonly string connString;

        public DbAction(int dbType, string connString)
        {
            this.dbType = dbType;
            this.connString = connString;
        }

        public int ExcuteNonQuery(string sql, object p = null)
        {
            using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
            {
                conn.ConnectionString = connString;
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var ret = conn.Execute(sql, param: p);
                return ret;
            }
        }

        public DataTable QueryDataTable(string sql, object p = null)
        {
            using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
            {
                conn.ConnectionString = connString;
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var ret = conn.ExecuteReader(sql, param: p);
                DataTable dt = new DataTable();
                dt.Load(ret);
                return dt;
            }
        }

        public IEnumerable<T> Query<T>(string sql, object p)
        {
            using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
            {
                conn.ConnectionString = connString;
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var ret = conn.Query<T>(sql, param: p);
                return ret;
            }
        }

        public T SingleResult<T>(string sql, object p = null)
        {
            using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
            {
                conn.ConnectionString = connString;
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var ret = conn.QuerySingle<T>(sql, param: p);
                return ret;
            }
        }
    }
}
