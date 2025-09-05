using LightORM.DbStruct;
using LightORM.Extension;

namespace LightORM.Implements;

public abstract class BaseDatabaseHandler<TWriter, TReader> : IDatabaseTableHandler
    where TWriter : WriteTableFromType
    where TReader : ReadTypeFromTable
{
    protected abstract TWriter Writer { get; }
    protected abstract TReader Reader { get; }

    public IEnumerable<string> GenerateDbTable<T>()
    {
        try
        {
            var info = typeof(T).CollectDbTableInfo();
            return Writer.BuildTableSql(info);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<IList<string>> GetTablesAsync() => Reader.GetTablesAsync();

    public Task<ReadedTable> GetTableStructAsync(string table) => Reader.GetTableStructAsync(table);

    protected static ISqlExecutor CreateSqlExecutor(IDatabaseProvider provider)
    {
        return new SqlExecutor.SqlExecutor(provider, 4, new());
    }
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

    public Task<IList<string>> GetTablesAsync()
    {
        throw new NotSupportedException();
    }
    public Task<ReadedTable> GetTableStructAsync(string table)
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

