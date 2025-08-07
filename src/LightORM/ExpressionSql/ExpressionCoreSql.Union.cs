using LightORM.Extension;

namespace LightORM.ExpressionSql;

partial class ExpressionCoreSql
{
    public IExpSelect<T> FromQuery<T>(IExpSelect<T> select)
    {
        return select.HandleSubQuery<T>();
    }

    public IExpSelect<T> FromTemp<T>(IExpTemp<T> temp)
    {
        var builder = new SelectBuilder();
        builder.HandleTempsRecursion(temp.SqlBuilder);
        builder.SelectedTables.Add(temp.ResultTable);
        return new SelectProvider1<T>(Ado, builder);
    }

    public IExpSelect<T> Union<T>(params IExpSelect<T>[] selects)
    {
        if (selects.Length == 0) LightOrmException.Throw("Union的数量为0");
        if (selects.Length == 1) return selects[0];
        var first = selects[0];
        var sub = first.HandleSubQuery<T>();
        for (var i = 1; i < selects.Length; i++)
        {
            var select = selects[i];
            first.SqlBuilder.AddUnion(select.SqlBuilder, false);
        }
        return sub;
    }

    public IExpSelect<T> UnionAll<T>(params IExpSelect<T>[] selects)
    {
        if (selects.Length == 0) LightOrmException.Throw("Union的数量为0");
        if (selects.Length == 1) return selects[0];
        var first = selects[0];
        var sub = first.HandleSubQuery<T>();
        for (var i = 1; i < selects.Length; i++)
        {
            var select = selects[i];
            first.SqlBuilder.AddUnion(select.SqlBuilder, true);
        }
        return sub;
    }
}
