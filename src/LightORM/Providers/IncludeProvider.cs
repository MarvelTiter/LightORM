using LightORM.Extension;
using System.Threading;

namespace LightORM.Providers
{
    internal class IncludeProvider<T1, TMember> : SelectProvider1<T1>, IExpInclude<T1, TMember>
    {
        public IncludeProvider(ISqlExecutor executor, SelectBuilder builder) : base(executor, builder) { }

        public override T1? First()
        {
            var t = base.First();
            if (t != null)
                SqlBuilder.IncludeContext.BindIncludeDatas(Executor, t);
            return t;
        }

        public override async Task<T1?> FirstAsync(CancellationToken cancellationToken = default)
        {
            var t = await base.FirstAsync(cancellationToken);
            if (t != null)
                // TODO async 版本
                SqlBuilder.IncludeContext.BindIncludeDatas(Executor, t);
            return t;
        }

        public override IEnumerable<T1> ToList()
        {
            var result = base.ToList().ToList();
            SqlBuilder.IncludeContext.BindIncludeDatas(Executor, result);
            return result;
        }

        public override async Task<IList<T1>> ToListAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.ToListAsync(cancellationToken);
            // TODO async 版本
            SqlBuilder.IncludeContext.BindIncludeDatas(Executor, result);
            return result;
        }
    }
}
