using LightORM.DbStruct;

namespace LightORM.Implements;

public abstract class WriteTableFromType<TOption> where TOption:TableOptions
{
    protected abstract string ConvertToDbType(TOption option, DbColumn type);
    protected abstract string BuildColumn(TOption option, DbColumn column);
    protected abstract string DbEmphasis(TOption option, string name);
    // TODO 待优化
    internal string DbEmphasisInternal(TOption option, string name) => DbEmphasis(option, name);
    public abstract IEnumerable<string> BuildTableSql(TOption option, DbTable table);

    protected static string GetIndexName(DbTable info, DbIndex index, int i)
    {
        return index.Name ?? $"IDX_{info.Name}_{string.Join("_", index.Columns)}_{i}";
    }

    protected static string GetPrimaryKeyName(string name, IEnumerable<DbColumn> pks)
    {
        return $"PK_{name}_{string.Join("_", pks.Select(c => c.Name))}";
    }

}

