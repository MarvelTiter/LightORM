using LightORM.DbStruct;

namespace LightORM.Implements;

public abstract class WriteTableFromType
{
    protected abstract string ConvertToDbType(TableGenerateOption option, DbColumn type);
    protected abstract string BuildColumn(TableGenerateOption option, DbColumn column);
    protected abstract string DbEmphasis(TableGenerateOption option, string name);
    // TODO 待优化
    internal string DbEmphasisInternal(string name)
    {
        return DbEmphasis(new TableGenerateOption(), name);
    }
    public abstract IEnumerable<string> BuildTableSql(TableGenerateOption option, DbTable table);

    protected static string GetIndexName(DbTable info, DbIndex index, int i)
    {
        return index.Name ?? $"IDX_{info.Name}_{string.Join("_", index.Columns)}_{i}";
    }

    protected static string GetPrimaryKeyName(string name, IEnumerable<DbColumn> pks)
    {
        return $"PK_{name}_{string.Join("_", pks.Select(c => c.Name))}";
    }

}

