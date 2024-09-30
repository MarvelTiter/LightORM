using System.Text;

namespace LightORM.Interfaces
{
    public interface ICustomDatabase
    {
        string Prefix { get; }
        string Emphasis { get; }
        void Paging(ISelectSqlBuilder builder, StringBuilder sql);
        string ReturnIdentitySql();
        ISqlMethodResolver MethodResolver { get; }
    }
}
