namespace LightORM;

public interface IDbInitial
{
    IDbInitial CreateTable<T>(params T[]? datas);
}
