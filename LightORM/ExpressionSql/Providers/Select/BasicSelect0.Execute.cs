using LightORM.Interfaces;
using LightORM.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.ExpressionSql.Providers.Select;

internal partial class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class, IExpSelect0
{
    public IEnumerable<T1> ToList()
    {
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQuery<T1>(args);
    }
    public IEnumerable<dynamic> ToDynamicList()
    {
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQuery(args);
    }
    public IEnumerable<TReturn> ToList<TReturn>()
    {
        //Expression<Func<TReturn, object>> exp = r => new { r };
        SelectHandle(selectBody);
        var args = BuildArgs();
        return InternalQuery<TReturn>(args);
    }
    //public IEnumerable<T1> ToList(Expression<Func<T1, object>> exp)
    //{
    //    SelectHandle(exp.Body);
    //    return InternalQuery<T1>();
    //}
    public DataTable ToDataTable()
    {
        //using var conn = dbConnect.CreateConnection();
        //var args = BuildArgs();
        //Life.BeforeExecute?.Invoke(args);
        //var ret = conn.ExecuteTable(args.Sql!, args.SqlParameter);
        //args.Done = true;
        //Life.AfterExecute?.Invoke(args);
        //return ret;
        throw new NotImplementedException();
    }

}
