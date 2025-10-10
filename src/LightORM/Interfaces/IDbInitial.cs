using System.Data.Common;

namespace LightORM;

public interface IDbInitial
{
    IDbInitial CreateTable<T>(params T[]? datas);
    // IDbInitial CreateOrUpdateTable<T>(params T[]? datas);
    IDbInitial Configuration(Action<TableGenerateOption> option);
}
