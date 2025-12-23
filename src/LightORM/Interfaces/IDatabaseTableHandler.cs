using LightORM.DbStruct;

namespace LightORM.Interfaces;

public interface IDatabaseTableHandler
{
    IEnumerable<string> GenerateDbTable<T>(TableGenerateOption option);
    string GetTablesSql();
    string GetTableStructSql(string table);
    bool ParseDataType(ReadedTableColumn column, out string type);
    string GetDropTableSql(string tableName);

}
