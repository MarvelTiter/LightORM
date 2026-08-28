using System.Runtime.CompilerServices;
using System.Threading;
namespace LightORM.Interfaces.ExpSql;

public interface ISql
{
    string ToSql();
    string ToSqlWithParameters();
}
public interface ISql<TPart, T> : ISql
{
    #region 自定义控制

    TPart NoQuoteIdentifiers();
    TPart QuoteIdentifiers();

    #endregion

    #region 日志输出辅助

    TPart TagWith(string tag);
    TPart TagWithCallSite(string tag, [CallerFilePath] string? filePath = null, [CallerMemberName] string? callMember = null, [CallerLineNumber] int? lineNum = null);

    #endregion

}

public interface ISqlWhereAndExecute<TPart, T> : ISql<TPart, T>
{
    TPart Where(Expression<Func<T, bool>> exp);
    TPart WhereIf(bool condition, Expression<Func<T, bool>> exp);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
