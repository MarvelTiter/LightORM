using LightORM.Builder;
using LightORM.Extension;
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
        public IncludeProvider(ISqlExecutor executor, SelectBuilder builder, IncludeContext context) : base(executor)
        {
            SqlBuilder = builder;
            IncludeContext = context;
        }

        public override T1? First()
        {
            var t = base.First();
            if (t != null)
                IncludeContext.BindIncludeDatas(Executor, t);
            return t;
        }

        public override async Task<T1?> FirstAsync()
        {
            var t = await base.FirstAsync();
            if (t != null)
                IncludeContext.BindIncludeDatas(Executor, t);
            return t;
        }

        public override IEnumerable<T1> ToList()
        {
            var result = base.ToList().ToList();
            IncludeContext.BindIncludeDatas(Executor, result);
            return result;
        }

        public override async Task<IList<T1>> ToListAsync()
        {
            var result = await base.ToListAsync();
            IncludeContext.BindIncludeDatas(Executor, result);
            return result;
        }
    }
}
