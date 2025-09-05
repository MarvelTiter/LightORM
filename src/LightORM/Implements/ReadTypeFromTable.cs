using LightORM.DbStruct;

namespace LightORM.Implements;

public abstract class ReadTypeFromTable
{
    public virtual async Task<ReadedTable> GetTableStructAsync(string table)
    {
        var columns = await GetTableColumnsAsync(table);
        var r = new ReadedTable() { TableName = table, Columns = columns };
        return r;
    }
    public abstract Task<IList<string>> GetTablesAsync();
    protected abstract Task<IList<ReadedTableColumn>> GetTableColumnsAsync(string table);
    protected abstract bool ParseDataType(ReadedTableColumn column, out string type);
}

