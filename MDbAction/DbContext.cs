using DExpSql;
using MDbAction;
using MDbAction.IServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MDbContext {
    public class DbContext : IDisposable {
        public ExpressionSql DbSet { get; private set; }
        private IDbAction dbAction;
        private string _sql => DbSet.SqlCaluse.Sql.ToString();
        private object _p {
            get {
                var temp = new Dictionary<string, object>();
                foreach (var item in DbSet.SqlCaluse.SqlParam) {
                    temp.Add(item.Key, item.Value);
                }
                return temp;
            }
        }

        private List<string> _sqls;
        private List<object> _ps;

        private static int DBType;
        public static DbContext Instance(IDbConnection conn) {
            return new DbContext(conn);
        }
        public static DbContext Instance(int type, IDbConnection conn) {
            return new DbContext(type, conn);
        }
        /// <summary>
        /// 初始化数据库类型 0 - SqlServer; 1 - Oracle; 2 - MySql
        /// </summary>
        /// <param name="dBType">0 - SqlServer; 1 - Oracle; 2 - MySql</param>
        public static void Init(int dBType) {
            DBType = dBType;
        }

        public DbContext(int type, IDbConnection connection) {
            dbAction = new DbAction(connection);
            DbSet = new ExpressionSql(type);
        }

        public DbContext(IDbConnection connection) {
            dbAction = new DbAction(connection);
            DbSet = new ExpressionSql(DBType);
        }

        public int Execute() {
            return dbAction.ExcuteNonQuery(_sql, _p);
        }

        public DataTable QueryDataTable() {
            return dbAction.QueryDataTable(_sql, _p);
        }

        public IEnumerable<T> Query<T>() {
            return dbAction.Query<T>(_sql, _p);
        }

        public T Single<T>() {
            return dbAction.SingleResult<T>(_sql, _p);
        }

        public int Execute(string sql, object p) {
            return dbAction.ExcuteNonQuery(sql, p);
        }

        public DataTable QueryDataTable(string sql, object p) {
            return dbAction.QueryDataTable(sql, p);
        }

        public IEnumerable<T> Query<T>(string sql, object p) {
            return dbAction.Query<T>(sql, p);
        }

        public T Single<T>(string sql, object p) {
            return dbAction.SingleResult<T>(sql, p);
        }

        public void AddTrans() {
            if (_sqls == null) _sqls = new List<string>();
            if (_ps == null) _ps = new List<object>();
            _sqls.Add(_sql);
            _ps.Add(_p);
        }

        public bool ExecuteTrans() {
            var result = dbAction.ExecuteTransaction(_sqls, _ps);
            _sqls.Clear();
            _ps.Clear();
            return result;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool m_disposed;

        protected virtual void Dispose(bool disposing) {
            if (!m_disposed) {
                if (disposing) {
                    // Release managed resources                   
                    this.DbSet = null;
                    dbAction = null;
                }

                // Release unmanaged resources

                m_disposed = true;
            }
        }

        ~DbContext() {
            Dispose(false);
        }
    }
}
