using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.ExpressionSql.Interface;
public interface ITransactionable
{
    void AddToTransaction();
}
