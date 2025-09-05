using LightORM.DbStruct;

namespace LightORM.Implements;

public abstract class WriteTableFromType(TableGenerateOption option)
{
    protected TableGenerateOption Option { get; } = option;
    protected abstract string ConvertToDbType(DbColumn type);
    protected abstract string BuildColumn(DbColumn column);
    protected abstract string DbEmphasis(string name);
    public abstract IEnumerable<string> BuildTableSql(DbTable table);

    protected static string GetIndexName(DbTable info, DbIndex index, int i)
    {
        return index.Name ?? $"IDX_{info.Name}_{string.Join("_", index.Columns)}_{i}";
    }

    protected static string GetPrimaryKeyName(string name, IEnumerable<DbColumn> pks)
    {
        return $"PK_{name}_{string.Join("_", pks.Select(c => c.Name))}";
    }

}

