using MDbAction.IServices;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using Dapper;

namespace MDbAction {
    /// <summary>
    /// 1. 查询
    /// 2. 执行Sql，返回受影响函数
    /// 3. 执行Sql，返回DataTable
    /// 4. 执行Sql，返回IEnumerable 
    /// </summary>
    public class DbAction : IDbAction {

        private readonly IDbConnection conn;
        public DbAction(IDbConnection conn) {
            this.conn = conn;
        }

        public int ExcuteNonQuery(string sql, object p) {
            try {
                int ret = conn.Execute(sql, param: p);
                return ret;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public DataTable QueryDataTable(string sql, object p) {
            try {
                DataTable dt = conn.ExecuteReader(sql, param: p);
                return dt;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public IEnumerable<T> Query<T>(string sql, object p) {
            try {
                IEnumerable<T> ret = conn.Query<T>(sql, param: p);
                return ret;
                
            } catch (Exception ex) {
                throw ex;
            }

        }

        public T SingleResult<T>(string sql, object p) {
            try {
                T ret = conn.QuerySingle<T>(sql, param: p);
                return ret;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public bool ExecuteTransaction(string[] sqls, object[] ps) {
            conn.Open();
            var tran = conn.BeginTransaction();
            try {
                if (sqls.Length != ps.Length)
                    throw new ArgumentException("SQL与参数不匹配");
                for (int i = 0; i < sqls.Length; i++) {
                    var sql = sqls[i];
                    var p = ps[i];
                    conn.Execute(sql, p, tran);
                }
                tran.Commit();
                return true;
            } catch (Exception ex) {
                tran.Rollback();
                conn.Close();
                throw ex;
            } finally {
                conn.Close();
            }
        }
    }
}
