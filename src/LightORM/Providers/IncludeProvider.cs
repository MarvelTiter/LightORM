using LightORM.Extension;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM.Providers
{
    internal class IncludeProvider<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        T1, TMember> : SelectProvider1<T1>, IExpInclude<T1, TMember>
    {
        public IncludeProvider(IContext dbContext, SelectBuilder builder) : base(dbContext, builder)
        {
        }

        public override T1? First()
        {
            var t = base.First();
            if (t is null)
            {
                return t;
            }

            if (AOTSupported)
            {
                SqlBuilder.MainTable.TableEntityInfo.HandleInclude(DbContext, t, SqlBuilder.Includes);
            }
            else
            {
                SqlBuilder.BindIncludeDatas(DbContext.Ado, t);
            }

            return t;
        }

        public override async Task<T1?> FirstAsync(CancellationToken cancellationToken = default)
        {
            var t = await base.FirstAsync(cancellationToken);
            if (t is null)
            {
                return t;
            }

            if (AOTSupported)
            {
                await SqlBuilder.MainTable.TableEntityInfo.HandleIncludeAsync(DbContext, t, SqlBuilder.Includes, cancellationToken);
            }
            else
            {
                SqlBuilder.BindIncludeDatas(DbContext.Ado, t);
            }

            return t;
        }

        public override IEnumerable<T1> ToList()
        {
            var result = base.ToList().ToList();

            if (AOTSupported)
            {
                if (result.Count > 0)
                {
                    SqlBuilder.MainTable.TableEntityInfo.HandleInclude(DbContext, result, SqlBuilder.Includes);
                }
            }
            else
            {
                SqlBuilder.BindIncludeDatas(DbContext.Ado, result);
            }

            return result;
        }

        public override async Task<IList<T1>> ToListAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.ToListAsync(cancellationToken);
            if (AOTSupported)
            {
                if (result.Count > 0)
                {
                    await SqlBuilder.MainTable.TableEntityInfo.HandleIncludeAsync(DbContext, result, SqlBuilder.Includes, cancellationToken);
                }
            }
            else
            {
                SqlBuilder.BindIncludeDatas(DbContext.Ado, result);
            }
            return result;
        }
    }
}