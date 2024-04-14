using LightORM.Builder;
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
        public IncludeInfo IncludeInfo { get; set; }
        SelectBuilder IExpInclude<T1, TMember>.SqlBuilder { get => SqlBuilder; set => SqlBuilder = value; }
        ISqlExecutor IExpInclude<T1, TMember>.Executor => Executor;

        public IncludeProvider(ISqlExecutor executor, SelectBuilder builder, IncludeInfo includeInfo) : base(executor)
        {
            SqlBuilder = builder;
            IncludeInfo = includeInfo;
        }

        public override IEnumerable<T1> ToList()
        {
            var result = base.ToList().ToList();
            return result;
        }

        public override async Task<IList<T1>> ToListAsync()
        {
            var result = await base.ToListAsync();

            return result;
        }
    }
}
