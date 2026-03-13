using LightORM.DbStruct;

namespace LightORM.Implements;

public abstract class WriteTableFromType
{
    protected abstract string ConvertToDbType(TableOptions option, DbColumn type);
    protected abstract string BuildColumn(TableOptions option, DbColumn column);
    protected abstract string DbEmphasis(TableOptions option, string name);
    // TODO 待优化
    internal string DbEmphasisInternal(TableOptions option, string name) => DbEmphasis(option, name);
    public abstract IEnumerable<string> BuildTableSql(TableOptions option, DbTable table);

    protected static string GetIndexName(DbTable info, DbIndex index, int i)
    {
        return index.Name ?? $"IDX_{info.Name}_{string.Join("_", index.Columns)}_{i}";
    }

    protected static string GetPrimaryKeyName(string name, IEnumerable<DbColumn> pks)
    {
        return $"PK_{name}_{string.Join("_", pks.Select(c => c.Name))}";
    }

}

