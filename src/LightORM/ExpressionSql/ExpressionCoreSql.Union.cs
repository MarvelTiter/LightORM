using LightORM.Extension;
using System.Diagnostics.CodeAnalysis;

namespace LightORM.ExpressionSql;

partial class ExpressionCoreSql
{
    public IExpSelect<T> FromQuery<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(IExpSelect<T> select)
    {
        return select.HandleSubQuery<T>();
    }

    public IExpSelect<T> FromTemp<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(IExpTemp<T> temp)
    {
        var builder = SelectBuilder.GetSelectBuilder();
        builder.HandleTempsRecursion(temp.SqlBuilder);
        builder.AddTableInfo(temp.ResultTable);
        return new SelectProvider1<T>(this, builder);
    }

    public IExpSelect<T> Union<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(params IExpSelect<T>[] selects)
    {
        if (selects.Length == 0)
        {
            throw new LightOrmException("Union的数量为0");
        }
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

    public IExpSelect<T> UnionAll<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    T>(params IExpSelect<T>[] selects)
    {
        if (selects.Length == 0)
        {
            throw new LightOrmException("Union的数量为0");
        }
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
