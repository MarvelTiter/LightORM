using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MDbContext.NewExpSql
{
    public partial class ExpressionSqlCore<T>
    {
        public IEnumerable<T> ToList()
        {
            throw new NotImplementedException();
        }

        public T ToSingle()
        {
            throw new NotImplementedException();
        }

        public DataTable ToDataTable()
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }
    }
}
