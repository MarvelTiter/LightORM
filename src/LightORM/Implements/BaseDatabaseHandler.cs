using LightORM.DbStruct;
using LightORM.Extension;

namespace LightORM.Implements;

public abstract partial class BaseDatabaseHandler<TOption> : IDatabaseTableHandler
    where TOption : TableOptions
{
    public abstract TOption Options { get; }
    public IEnumerable<string> GenerateDbTable<T>()
    {
        try
        {
            var info = typeof(T).CollectDbTableInfo();
            return BuildTableSql(Options, info);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public abstract string GetTablesSql();

    public abstract string GetTableStructSql(string table);
    public abstract bool ParseDataType(ReadedTableColumn column, out string type);
    public virtual IEnumerable<string> GetDropTableSql(DbTable table)
    {
        yield return $"DROP TABLE {DbEmphasisInternal(Options, table.Name)}";
    }
}

partial class BaseDatabaseHandler<TOption>
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

//[Obsolete]
//public abstract class BaseDatabaseHandler : IDatabaseTableHandler
//{
//    protected abstract string ConvertToDbType(DbColumn type);
//    protected abstract string BuildColumn(DbColumn column);
//    protected abstract string DbEmphasis(string name);
//    protected abstract IEnumerable<string> BuildSql(DbTable table);
//    protected TableGenerateOption Option { get; }

//    public BaseDatabaseHandler(TableGenerateOption option)
//    {
//        Option = option;
//    }

//    public IEnumerable<string> GenerateDbTable<T>(TableGenerateOption option)
//    {
//        try
//        {
//            var info = typeof(T).CollectDbTableInfo();
//            return BuildSql(info);
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }

//    public string GetTablesSql()
//    {
//        throw new NotImplementedException();
//    }

//    public string GetTableStructSql(string table)
//    {
//        throw new NotImplementedException();
//    }

//    public bool ParseDataType(ReadedTableColumn column, out string type)
//    {
//        throw new NotImplementedException();
//    }

//    protected static string GetIndexName(DbTable info, DbIndex index, int i)
//    {
//        return index.Name ?? $"IDX_{info.Name}_{string.Join("_", index.Columns)}_{i}";
//    }

//    protected static string GetPrimaryKeyName(string name, IEnumerable<DbColumn> pks)
//    {
//        return $"PK_{name}_{string.Join("_", pks.Select(c => c.Name))}";
//    }

//    public string GetDropTableSql(string tableName)
//    {
//        throw new NotImplementedException();
//    }
//}