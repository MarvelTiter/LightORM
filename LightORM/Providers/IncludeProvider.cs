using LightORM.Providers.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers
{
    internal class IncludeProvider<T1, TMember> : SelectProvider0<IExpInclude<T1, TMember>, T1>, IExpInclude<T1, TMember>
    {
        public ISqlExecutor SqlExecutor => Executor;
        public IncludeProvider(ISqlExecutor executor) : base(executor)
        {
        }

        //public IExpInclude<T1, TMember> ThenInclude(Expression<Func<TMember, object>> exp)
        //{
        //    var result = exp.Resolve(SqlResolveOptions.Select);

        //    return this;
        //}
    }
}
