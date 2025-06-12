using LightORM.Extension;
using LightORM.Providers.Select;

namespace LightORM;

public static class ExpressionContextExtension
{
    /// <summary>
    /// 批量插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ado"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public static int BulkCopy<T>(this ISqlExecutor ado, IEnumerable<T> datas)
    {
        var table = TableContext.GetTableInfo<T>();
        var dt = new DataTable();
        foreach (var col in table.Columns)
        {
            if (col.IsNotMapped) continue;
            dt.Columns.Add(col.ColumnName);
        }
        foreach (var item in datas)
        {
            if (item is null) continue;
            var row = dt.NewRow();
            foreach (var col in table.Columns)
            {
                if (col.IsNotMapped) continue;
                row[col.ColumnName] = col.GetValue(item);
            }
            dt.Rows.Add(row);
        }
        return ado.BulkCopy(dt);
    }

    /// <summary>
    /// 批量插入
    /// </summary>
    /// <param name="ado"></param>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    public static int BulkCopy(this ISqlExecutor ado, DataTable dataTable)
    {
        return ado.Database.BulkCopy(dataTable);
    }

    private static void SwitchDb<T>(this IExpressionContext context)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            context.SwitchDatabase(table.TargetDatabase);
        }
    }

    public static IExpSelect<T> SelectWithAttr<T>(this IExpressionContext context)
    {
        context.SwitchDb<T>();
        return context.Select<T>();
    }
    public static IExpInsert<T> InsertWithAttr<T>(this IExpressionContext context, T entity)
    {
        context.SwitchDb<T>();
        return context.Insert<T>(entity);
    }
    public static IExpInsert<T> InsertWithAttr<T>(this IExpressionContext context, IEnumerable<T> entities)
    {
        context.SwitchDb<T>();
        return context.Insert<T>(entities);
    }
    public static IExpUpdate<T> UpdateWithAttr<T>(this IExpressionContext context)
    {
        context.SwitchDb<T>();
        return context.Update<T>();
    }
    public static IExpUpdate<T> UpdateWithAttr<T>(this IExpressionContext context, T entity)
    {
        context.SwitchDb<T>();
        return context.Update<T>(entity);
    }
    public static IExpUpdate<T> UpdateWithAttr<T>(this IExpressionContext context, IEnumerable<T> entities)
    {
        context.SwitchDb<T>();
        return context.Update<T>(entities);
    }
    public static IExpDelete<T> DeleteWithAttr<T>(this IExpressionContext context)
    {
        context.SwitchDb<T>();
        return context.Delete<T>();
    }
    public static IExpDelete<T> DeleteWithAttr<T>(this IExpressionContext context, T entity)
    {
        context.SwitchDb<T>();
        return context.Delete<T>(entity);
    }
    public static IExpDelete<T> DeleteWithAttr<T>(this IExpressionContext context, IEnumerable<T> entities)
    {
        throw new NotImplementedException();
    }

    public static ISingleScopedExpressionContext CreateMainDbScope(this IExpressionContext context)
    {
        return context.CreateScoped("MainDb");
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

public static class ScopedExpressionContextExtensions
{
    private static void SwitchDb<T>(this IScopedExpressionContext context)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            context.SwitchDatabase(table.TargetDatabase);
        }
    }

    public static IExpSelect<T> SelectWithAttr<T>(this IScopedExpressionContext context)
    {
        context.SwitchDb<T>();
        return context.Select<T>();
    }
    public static IExpInsert<T> InsertWithAttr<T>(this IScopedExpressionContext context, T entity)
    {
        context.SwitchDb<T>();
        return context.Insert<T>(entity);
    }
    public static IExpInsert<T> InsertWithAttr<T>(this IScopedExpressionContext context, IEnumerable<T> entities)
    {
        context.SwitchDb<T>();
        return context.Insert<T>(entities);
    }
    public static IExpUpdate<T> UpdateWithAttr<T>(this IScopedExpressionContext context)
    {
        context.SwitchDb<T>();
        return context.Update<T>();
    }
    public static IExpUpdate<T> UpdateWithAttr<T>(this IScopedExpressionContext context, T entity)
    {
        context.SwitchDb<T>();
        return context.Update<T>(entity);
    }
    public static IExpUpdate<T> UpdateWithAttr<T>(this IScopedExpressionContext context, IEnumerable<T> entities)
    {
        context.SwitchDb<T>();
        return context.Update<T>(entities);
    }
    public static IExpDelete<T> DeleteWithAttr<T>(this IScopedExpressionContext context)
    {
        context.SwitchDb<T>();
        return context.Delete<T>();
    }
    public static IExpDelete<T> DeleteWithAttr<T>(this IScopedExpressionContext context, T entity)
    {
        context.SwitchDb<T>();
        return context.Delete<T>(entity);
    }
    public static IExpDelete<T> DeleteWithAttr<T>(this IScopedExpressionContext context, IEnumerable<T> entities)
    {
        throw new NotImplementedException();
    }
}

