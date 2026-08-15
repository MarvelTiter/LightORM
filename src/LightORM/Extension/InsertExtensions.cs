using System.Threading;

namespace LightORM;

public static class InsertExtensions
{
    extension<T>(IExpInsert<T> insert)
    {
        public int InsertEach(IEnumerable<T> entities)
        {
            int count = 0;
            foreach (var entity in entities)
            {
                insert.SetTargetObject(entity);
                count += insert.Execute();
            }
            return count;
        }

        public async Task<int> InsertEachAsync(IEnumerable<T> entities
            , CancellationToken cancellationToken = default)
        {
            int count = 0;
            foreach (var entity in entities)
            {
                insert.SetTargetObject(entity);
                count += await insert.ExecuteAsync(cancellationToken: cancellationToken);
            }
            return count;
        }

        public IExpInsert<T> OrUpdate()
        {
            return insert.OrUpdate<int>(null, null);
        }
        public IExpInsert<T> OrUpdate(Expression<Func<T, bool>> wherePredicate)
        {
            return insert.OrUpdate<int>(wherePredicate, null);
        }
    }
}
