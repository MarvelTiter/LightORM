using LightORM.DbStruct;

namespace LightORM.Interfaces;

public interface IDatabaseTableHandler
{
    TableOptions Options { get; }
    IEnumerable<string> GenerateDbTable<T>();
    string GetTablesSql();
    string GetTableStructSql(string table);
    bool ParseDataType(ReadedTableColumn column, out string type);
    string GetDropTableSql(string tableName);

}
