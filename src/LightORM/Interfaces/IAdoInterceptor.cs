using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Interfaces;

public interface IAdoInterceptor
{
    void OnPrepareCommand(SqlExecuteContext context);
    void BeforeExecute(SqlExecuteContext context);
    void AfterExecute(SqlExecuteContext context);
    void OnException(SqlExecuteExceptionContext context);
}
