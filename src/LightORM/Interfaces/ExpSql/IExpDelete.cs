namespace LightORM.Interfaces.ExpSql;

public interface IExpDelete<T> : ISql<IExpDelete<T>, T>
{
    /// <summary>
    /// 全表删除
    /// </summary>
    /// <param name="truncate"></param>
    /// <returns></returns>
    IExpDelete<T> FullDelete(bool truncate = false);
}
