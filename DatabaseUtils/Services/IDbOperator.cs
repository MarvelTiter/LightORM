using DatabaseUtils.Models;

namespace DatabaseUtils.Services
{
    public interface IDbOperator
    {
        Task<IList<DatabaseTable>> GetTablesAsync();
        Task<IList<TableColumn>> GetTableStructAsync(string table);
        bool ParseDataType(TableColumn column, out string type);
    }
}
