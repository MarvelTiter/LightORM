namespace LightORM.Interfaces;

public interface IDatabaseTableHandler
{
    IEnumerable<string> GenerateDbTable<T>();
    void SaveDbTableStruct();
    //string ConvertToDbType(DbColumn type);
}
