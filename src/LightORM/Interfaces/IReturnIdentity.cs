using System.Text;

namespace LightORM.Interfaces
{
    /// <summary>
    /// [可选能力接口] 方言是否能把"自增主键回读"语句拼接到 INSERT 语句末尾（如 SqlServer 的 SCOPE_IDENTITY()、MySql/Dameng 的 @@IDENTITY、Sqlite 的 LAST_INSERT_ROWID()）。
    /// 仅具备该特化的方言(SqlServer/MySql/Dameng/Sqlite)声明并实现；
    /// Oracle/PostgreSQL/KingbaseES 等不具备，框架在使用 <see cref="IExpInsert{T}.ReturnIdentity"/> 时经能力探测(type-cast)判断，转换失败即明确报"不支持"。
    /// </summary>
    public interface IReturnIdentity
    {
        void ReturnIdentitySql(StringBuilder sql);
    }
}
