using DatabaseUtils.Models;

namespace DatabaseUtils.Services
{
    public interface IDbOperator
    {
        Task<IList<DatabaseTable>> GetTablesAsync();
        Task<IList<TableColumn>> GetTableStructAsync(string table);
    }
}
