using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbAction.IServices
{
    public interface IDbAction
    {
        T SingleResult<T>(string sql, object p = null);
        int ExcuteNonQuery(string sql, object p = null);
        DataTable QueryDataTable(string sql, object p = null);
        IEnumerable<T> Query<T>(string sql, object p);
    }
}
