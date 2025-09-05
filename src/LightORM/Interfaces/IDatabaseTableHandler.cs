using LightORM.DbStruct;

namespace LightORM.Interfaces;

public interface IDatabaseTableHandler
{
    IEnumerable<string> GenerateDbTable<T>();
    string GetTablesSql();
    Task<ReadedTable> GetTableStructAsync(string table);
    //void SaveDbTableStruct();

    //string ConvertToDbType(DbColumn type);
}
