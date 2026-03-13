using System.Reflection;
using System.Text;

namespace LightORM.Interfaces;

public interface IJsonColumnHandler
{
    void SelectJson(StringBuilder sql, Stack<MemberInfo> members, ResolveContext context, TableInfo table);
}
