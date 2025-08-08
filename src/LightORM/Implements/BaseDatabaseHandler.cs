using LightORM.DbStruct;
using LightORM.Extension;

namespace LightORM.Implements;

public abstract class BaseDatabaseHandler : IDatabaseTableHandler
{
    protected abstract string ConvertToDbType(DbColumn type);
    protected abstract string BuildColumn(DbColumn column);
    protected abstract string DbEmphasis(string name);
    protected abstract IEnumerable<string> BuildSql(DbTable table);
    protected TableGenerateOption Option { get; }
    public BaseDatabaseHandler(TableGenerateOption option)
    {
        Option = option;
    }
    public IEnumerable<string> GenerateDbTable<T>()
    {
        try
        {
            var info = typeof(T).CollectDbTableInfo();
            return BuildSql(info);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public virtual void SaveDbTableStruct()
    {
        throw new NotSupportedException();
    }

    protected static string GetIndexName(DbTable info, DbIndex index, int i)
    {
        return index.Name ?? $"IDX_{info.Name}_{string.Join("_", index.Columns)}_{i}";
    }

    protected static string GetPrimaryKeyName(string name, IEnumerable<DbColumn> pks)
    {
        return $"PK_{name}_{string.Join("_", pks.Select(c => c.Name))}";
    }
}


