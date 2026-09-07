using System.Collections.Concurrent;

namespace LightORM.Utils;

public class LightOrmTableContextBase
{
    private static readonly ConcurrentDictionary<Type, ITableEntityInfo> tableInfos = [];

    private static readonly ConcurrentDictionary<Type, Action<ITableColumnInfo, object, object?>> table_sets = [];

    private static readonly ConcurrentDictionary<Type, Func<ITableColumnInfo, object, object?>> table_gets = [];

    protected static ConcurrentDictionary<Type, ITableEntityInfo> TableInfos => tableInfos;

    protected static ConcurrentDictionary<Type, Action<ITableColumnInfo, object, object?>> Table_sets => table_sets;

    protected static ConcurrentDictionary<Type, Func<ITableColumnInfo, object, object?>> Table_gets => table_gets;

    public static void AddTable(Type type, ITableEntityInfo entityInfo)
    {
        _ = tableInfos.GetOrAdd(type, entityInfo);
    }
}
