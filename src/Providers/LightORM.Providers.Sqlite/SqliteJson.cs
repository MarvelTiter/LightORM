using LightORM.Interfaces;
using LightORM.Models;
using System.Reflection;
using System.Text;

namespace LightORM.Providers.Sqlite;

internal class SqliteJson : IJsonColumnHandler
{
    public static readonly SqliteJson Instance = new();
    public void SelectJson(StringBuilder sql, Stack<MemberInfo> members, ResolveContext context, TableInfo table)
    {

    }
}
