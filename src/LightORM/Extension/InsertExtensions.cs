using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM;

public static class InsertExtensions
{
    public static int InsertEach<T>(this IExpInsert<T> insert, IEnumerable<T> entities)
    {
        if (insert is null) throw new ArgumentNullException(nameof(insert));
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        int count = 0;
        foreach (var entity in entities)
        {
            insert.SetTargetObject(entity);
            count += insert.Execute();
        }
        return count;
    }

    public static async Task<int> InsertEachAsync<T>(this IExpInsert<T> insert, IEnumerable<T> entities)
    {
        if (insert is null) throw new ArgumentNullException(nameof(insert));
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        int count = 0;
        foreach (var entity in entities)
        {
            insert.SetTargetObject(entity);
            count += await insert.ExecuteAsync();
        }
        return count;
    }
}
