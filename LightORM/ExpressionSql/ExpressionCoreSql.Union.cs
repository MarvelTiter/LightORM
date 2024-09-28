using LightORM.Extension;

namespace LightORM.ExpressionSql;

partial class ExpressionCoreSql
{
    public IExpSelect<TResult> Union<TResult>(params IExpSelect<TResult>[] selects)
    {
        if (selects.Length == 0) throw new LightOrmException("Union的数量为0");
        if (selects.Length == 1) return selects[0];
        var first = selects[0];
        var sub = first.HandleSubQuery<TResult>();
        for (var i = 1; i < selects.Length; i++)
        {
            var select = selects[i];
            first.SqlBuilder.AddUnion(select.SqlBuilder, false);
        }
        return sub;
    }

    public IExpSelect<TResult> UnionAll<TResult>(params IExpSelect<TResult>[] selects)
    {
        if (selects.Length == 0) throw new LightOrmException("Union的数量为0");
        if (selects.Length == 1) return selects[0];
        var first = selects[0];
        var sub = first.HandleSubQuery<TResult>();
        for (var i = 1; i < selects.Length; i++)
        {
            var select = selects[i];
            first.SqlBuilder.AddUnion(select.SqlBuilder, true);
        }
        return sub;
    }


}
