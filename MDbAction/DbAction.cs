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
            try
            {
                using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
                {
                    conn.ConnectionString = connString;
                    IDbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    int ret;
                    if (p == null)
                        ret = conn.Execute(sql);
                    else
                        ret = conn.Execute(sql, param: p);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable QueryDataTable(string sql, object p = null)
        {
            try
            {
                using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
                {
                    conn.ConnectionString = connString;
                    IDbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    IDataReader ret;
                    if (p == null)
                        ret = conn.ExecuteReader(sql);
                    else
                        ret = conn.ExecuteReader(sql, param: p);
                    DataTable dt = new DataTable();
                    dt.Load(ret);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<T> Query<T>(string sql, object p)
        {
            try
            {
                using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
                {
                    conn.ConnectionString = connString;
                    IDbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    IEnumerable<T> ret;
                    if (p == null)
                        ret = conn.Query<T>(sql);
                    else
                        ret = conn.Query<T>(sql, param: p);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public T SingleResult<T>(string sql, object p = null)
        {
            try
            {
                using (IDbConnection conn = DbConnectionFactory.CreateConnect(dbType))
                {
                    conn.ConnectionString = connString;
                    IDbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    T ret;
                    if (p == null)
                        ret = conn.QuerySingle<T>(sql);
                    else
                        ret = conn.QuerySingle<T>(sql, param: p);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
