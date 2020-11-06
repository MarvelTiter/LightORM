using DExpSql;
using MDbAction;
using MDbAction.IServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MDbContext
{
    public class DbContext : IDisposable
    {
        public ExpressionSql DbSet { get; private set; }
        private IDbAction dbAction;
        private string _sql => DbSet.SqlCaluse.Sql.ToString();
        private object _p => DbSet.SqlCaluse.SqlParam;
        private static int DBType;
        private static string connectString;
        public static DbContext Instance
        {
            get
            {
                return new DbContext();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBType">0 - SqlServer; 1 - Oracle; 2 - MySql</param>
        /// <param name="connString"></param>
        public static void Init(int dBType, string connString)
        {
            DBType = dBType;
            connectString = connString;
        }

        public DbContext(int type, string connString)
        {
            dbAction = new DbAction(type, connString);
            DbSet = new ExpressionSql(type);
        }

        public DbContext()
        {
            dbAction = new DbAction(DBType, connectString);
            DbSet = new ExpressionSql(DBType);
        }

        public int Excute()
        {
            return dbAction.ExcuteNonQuery(_sql, _p);
        }

        public DataTable QueryDataTable()
        {
            return dbAction.QueryDataTable(_sql, _p);
        }

        public IEnumerable<T> Query<T>()
        {
            return dbAction.Query<T>(_sql, _p);
        }

        public T Single<T>()
        {
            return dbAction.SingleResult<T>(_sql, _p);
        }

        public int Excute(string sql, object p)
        {
            return dbAction.ExcuteNonQuery(sql, p);
        }

        public DataTable QueryDataTable(string sql, object p)
        {
            return dbAction.QueryDataTable(sql, p);
        }

        public IEnumerable<T> Query<T>(string sql, object p)
        {
            return dbAction.Query<T>(sql, p);
        }

        public T Single<T>(string sql, object p)
        {
            return dbAction.SingleResult<T>(sql, p);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool m_disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // Release managed resources
                    this.DbSet = null;
                    dbAction = null;
                }

                // Release unmanaged resources

                m_disposed = true;
            }
        }

        ~DbContext()
        {
            Dispose(false);
        }
    }
}
