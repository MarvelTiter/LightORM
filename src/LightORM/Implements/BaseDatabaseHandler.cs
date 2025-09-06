using LightORM.DbStruct;
using LightORM.Extension;

namespace LightORM.Implements;

public abstract class BaseDatabaseHandler<TWriter> : IDatabaseTableHandler
    where TWriter : WriteTableFromType, new()
{
    protected TWriter Writer { get; } = new();

    public IEnumerable<string> GenerateDbTable<T>(TableGenerateOption option)
    {
        try
        {
            var info = typeof(T).CollectDbTableInfo();
            return Writer.BuildTableSql(option, info);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public abstract string GetTablesSql();

    public abstract string GetTableStructSql(string table);
    public abstract bool ParseDataType(ReadedTableColumn column, out string type);
}

[Obsolete]
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

    public IEnumerable<string> GenerateDbTable<T>(TableGenerateOption option)
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

    public string GetTablesSql()
    {
        throw new NotImplementedException();
    }

    public string GetTableStructSql(string table)
    {
        throw new NotImplementedException();
    }

    public bool ParseDataType(ReadedTableColumn column, out string type)
    {
        throw new NotImplementedException();
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