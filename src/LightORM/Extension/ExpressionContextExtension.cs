using LightORM.Extension;
using LightORM.Providers.Select;

namespace LightORM;

public static class ExpressionContextExtension
{
    public static int BulkCopy(this ISqlExecutor ado, DataTable dataTable)
    {
        return ado.Database.BulkCopy(dataTable);
    }

    private static void HandleFromTemp(SelectBuilder sqlbuilder, params IExpTemp[] temps)
    {
        foreach (var temp in temps)
        {
            sqlbuilder.HandleTempsRecursion(temp.SqlBuilder);
            sqlbuilder.SelectedTables.Add(temp.ResultTable);
        }
    }
    public static IExpSelect<TTemp1, TTemp2> FromTemp<TTemp1, TTemp2>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2)
    {
        var builder = new SelectBuilder(temp1.SqlBuilder.DbType);
        HandleFromTemp(builder, temp1, temp2);
        return new SelectProvider2<TTemp1, TTemp2>(context.Ado, builder);
    }

    public static IExpSelect<TTemp1, TTemp2, TTemp3> FromTemp<TTemp1, TTemp2, TTemp3>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3)
    {
        var builder = new SelectBuilder(temp1.SqlBuilder.DbType);
        HandleFromTemp(builder, temp1, temp2, temp3);
        return new SelectProvider3<TTemp1, TTemp2, TTemp3>(context.Ado, builder);
    }

    public static IExpSelect<TTemp1, TTemp2, TTemp3, TTemp4> FromTemp<TTemp1, TTemp2, TTemp3, TTemp4>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3, IExpTemp<TTemp2> temp4)
    {
        var builder = new SelectBuilder(temp1.SqlBuilder.DbType);
        HandleFromTemp(builder, temp1, temp2, temp3, temp4);
        return new SelectProvider4<TTemp1, TTemp2, TTemp3, TTemp4>(context.Ado, builder);
    }

    public static IExpSelect<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> FromTemp<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3, IExpTemp<TTemp2> temp4, IExpTemp<TTemp2> temp5)
    {
        var builder = new SelectBuilder(temp1.SqlBuilder.DbType);
        HandleFromTemp(builder, temp1, temp2, temp3, temp4, temp5);
        return new SelectProvider5<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(context.Ado, builder);
    }
}

