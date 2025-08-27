using LightORM.Extension;
using LightORM.Repository;

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
        var dt = new DataTable(table.TableName);
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

    /// <summary>
    /// 获取仓储对象<see cref="ILightOrmRepository{TEntity}"/>
    /// <para>注意释放对象</para>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static ILightOrmRepository<TEntity> GetRepository<TEntity>(this IExpressionContext context)
        where TEntity : class, new()
    {
        return new DefaultRepository<TEntity>(context);
    }

    private static ITransientExpressionContext SwitchDb<T>(this IExpressionContext context)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase is null)
        {
            throw new LightOrmException("实体上没有设置DatabaseKey，无法自动切换数据库");
        }
        return context.SwitchDatabase(table.TargetDatabase);
    }

    public static IExpSelect<T> SelectWithAttr<T>(this IExpressionContext context)
        => context.SwitchDb<T>().Select<T>();

    public static IExpInsert<T> InsertWithAttr<T>(this IExpressionContext context, T entity)
        => context.SwitchDb<T>().Insert<T>(entity);

    public static IExpInsert<T> InsertWithAttr<T>(this IExpressionContext context, params T[] entities)
        => context.SwitchDb<T>().Insert<T>(entities);

    public static IExpUpdate<T> UpdateWithAttr<T>(this IExpressionContext context)
        => context.SwitchDb<T>().Update<T>();

    public static IExpUpdate<T> UpdateWithAttr<T>(this IExpressionContext context, T entity)
     => context.SwitchDb<T>().Update<T>(entity);

    public static IExpUpdate<T> UpdateWithAttr<T>(this IExpressionContext context, params T[] entities)
        => context.SwitchDb<T>().Update<T>(entities);

    public static IExpDelete<T> DeleteWithAttr<T>(this IExpressionContext context)
        => context.SwitchDb<T>().Delete<T>();

    public static IExpDelete<T> DeleteWithAttr<T>(this IExpressionContext context, T entity)
        => context.SwitchDb<T>().Delete<T>(entity);

    public static IExpDelete<T> DeleteWithAttr<T>(this IExpressionContext context, params T[] entities)
        => context.SwitchDb<T>().Delete<T>(entities);

    public static ISingleScopedExpressionContext CreateMainDbScoped(this IExpressionContext context)
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
        var builder = new SelectBuilder();
        HandleFromTemp(builder, temp1, temp2);
        return new SelectProvider2<TTemp1, TTemp2>(context.Ado, builder);
    }

    public static IExpSelect<TTemp1, TTemp2, TTemp3> FromTemp<TTemp1, TTemp2, TTemp3>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3)
    {
        var builder = new SelectBuilder();
        HandleFromTemp(builder, temp1, temp2, temp3);
        return new SelectProvider3<TTemp1, TTemp2, TTemp3>(context.Ado, builder);
    }

    public static IExpSelect<TTemp1, TTemp2, TTemp3, TTemp4> FromTemp<TTemp1, TTemp2, TTemp3, TTemp4>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3, IExpTemp<TTemp2> temp4)
    {
        var builder = new SelectBuilder();
        HandleFromTemp(builder, temp1, temp2, temp3, temp4);
        return new SelectProvider4<TTemp1, TTemp2, TTemp3, TTemp4>(context.Ado, builder);
    }

    public static IExpSelect<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> FromTemp<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(this IExpressionContext context
        , IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3, IExpTemp<TTemp2> temp4, IExpTemp<TTemp2> temp5)
    {
        var builder = new SelectBuilder();
        HandleFromTemp(builder, temp1, temp2, temp3, temp4, temp5);
        return new SelectProvider5<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(context.Ado, builder);
    }
}

public static class ScopedExpressionContextExtensions
{
    private static IScopedExpressionContext SwitchDb<T>(this IScopedExpressionContext context)
    {
        var table = TableContext.GetTableInfo<T>();
        if (table.TargetDatabase != null)
        {
            return context.SwitchDatabase(table.TargetDatabase);
        }
        throw new LightOrmException("实体上没有设置DatabaseKey，无法自动切换数据库");
    }

    public static IExpSelect<T> SelectWithAttr<T>(this IScopedExpressionContext context)
        => context.SwitchDb<T>().Select<T>();

    public static IExpInsert<T> InsertWithAttr<T>(this IScopedExpressionContext context, T entity)
        => context.SwitchDb<T>().Insert<T>(entity);


    public static IExpInsert<T> InsertWithAttr<T>(this IScopedExpressionContext context, params T[] entities)
        => context.SwitchDb<T>().Insert<T>(entities);

    public static IExpUpdate<T> UpdateWithAttr<T>(this IScopedExpressionContext context)
        => context.SwitchDb<T>().Update<T>();

    public static IExpUpdate<T> UpdateWithAttr<T>(this IScopedExpressionContext context, T entity)
        => context.SwitchDb<T>().Update<T>(entity);

    public static IExpUpdate<T> UpdateWithAttr<T>(this IScopedExpressionContext context, params T[] entities)
        => context.SwitchDb<T>().Update<T>(entities);

    public static IExpDelete<T> DeleteWithAttr<T>(this IScopedExpressionContext context)
        => context.SwitchDb<T>().Delete<T>();

    public static IExpDelete<T> DeleteWithAttr<T>(this IScopedExpressionContext context, T entity)
        => context.SwitchDb<T>().Delete<T>(entity);

    public static IExpDelete<T> DeleteWithAttr<T>(this IScopedExpressionContext context, params T[] entities)
        => context.SwitchDb<T>().Delete<T>(entities);
}

