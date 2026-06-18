using LightORM.Extension;
using LightORM.Providers;
using LightORM.Repository;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

public static class ExpressionContextExtension
{
    /// <param name="ado"></param>
    extension(ISqlExecutor ado)
    {
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <returns></returns>
        public int BulkCopy<T>(IEnumerable<T> datas)
        {
            var table = TableContext.GetTableInfo<T>();
            var dt = new DataTable(table.TableName);
            Dictionary<string, Type> addedColumns = [];
            foreach (var col in table.Columns)
            {
                if (col.IsNotMapped || col.IsAggregated || col.IsNavigate) continue;
                var propType = table.Type?.GetProperty(col.PropertyName)?.PropertyType;
                if (propType is null)
                {
                    continue;
                }

                propType = Nullable.GetUnderlyingType(propType) ?? propType;
                var isBool = propType == typeof(bool);
                dt.Columns.Add(col.ColumnName, isBool ? typeof(object) : propType);
                addedColumns.Add(col.ColumnName, propType);
            }

            foreach (var item in datas)
            {
                if (item is null) continue;
                var row = dt.NewRow();
                foreach (var col in table.Columns)
                {
                    if (addedColumns.TryGetValue(col.ColumnName, out var type))
                    {
                        var value = col.GetValue(item);
                        if (value is null)
                        {
                            row[col.ColumnName] = DBNull.Value;
                            continue;
                        }

                        if (value is bool b)
                        {
                            // bool类型特殊处理
                            row[col.ColumnName] = ado.Database.DatabaseAdapter.HandleBooleanValueForBulkCopy(b);
                            continue;
                        }

                        row[col.ColumnName] = Convert.ChangeType(value, type);
                    }
                }

                dt.Rows.Add(row);
            }

            return ado.BulkCopy(dt);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public int BulkCopy(DataTable dataTable)
        {
            return ado.Database.BulkCopy(dataTable);
        }
    }

    extension(IContext context)
    {
        public IExpSelect<T> Select<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            T>(string tableName)
        {
            return new SelectProvider1<T>(tableName, context);
        }

        public IExpInsert<T> Insert<T>(string tableName, params T[] values)
        {
            InsertProvider<T>? p;
            if (values.Length == 1)
            {
                p = new InsertProvider<T>(context.Ado, values[0]);
            }
            else
            {
                p = new InsertProvider<T>(context.Ado, values);
            }

            p.UpdateTableName(tableName);
            return p;
        }

        public IExpUpdate<T> Update<T>(string tableName, params T[] values)
        {
            UpdateProvider<T>? p;
            if (values.Length == 1)
            {
                p = new UpdateProvider<T>(context.Ado, values[0]);
            }
            else
            {
                p = new UpdateProvider<T>(context.Ado, values);
            }

            p.UpdateTableName(tableName);
            return p;
        }

        public IExpDelete<T> Delete<T>(string tableName, params T[] values)
        {
            DeleteProvider<T>? p;
            if (values.Length == 1)
            {
                p = new DeleteProvider<T>(context.Ado, values[0]);
            }
            else
            {
                p = new DeleteProvider<T>(context.Ado, values);
            }

            p.UpdateTableName(tableName);
            return p;
        }
    }

    extension(IExpressionContext context)
    {
        /// <summary>
        /// 获取仓储对象<see cref="ILightOrmRepository{TEntity}"/>
        /// <para>注意释放对象</para>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ILightOrmRepository<TEntity> GetRepository<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            TEntity>()
            where TEntity : class, new()
        {
            return new DefaultRepository<TEntity>(context);
        }

        private ITransientExpressionContext SwitchDb<T>()
        {
            var table = TableContext.GetTableInfo<T>();
            if (table.TargetDatabase is null)
            {
                throw new LightOrmException("实体上没有设置DatabaseKey，无法自动切换数据库");
            }

            return context.SwitchDatabase(table.TargetDatabase);
        }

        public IExpSelect<T> SelectWithAttr<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            T>()
            => context.SwitchDb<T>().Select<T>();

        public IExpInsert<T> InsertWithAttr<T>(T entity)
            => context.SwitchDb<T>().Insert(entity);

        public IExpInsert<T> InsertWithAttr<T>(params T[] entities)
            => context.SwitchDb<T>().Insert(entities);

        public IExpUpdate<T> UpdateWithAttr<T>()
            => context.SwitchDb<T>().Update<T>();

        public IExpUpdate<T> UpdateWithAttr<T>(T entity)
            => context.SwitchDb<T>().Update(entity);

        public IExpUpdate<T> UpdateWithAttr<T>(params T[] entities)
            => context.SwitchDb<T>().Update(entities);

        public IExpDelete<T> DeleteWithAttr<T>()
            => context.SwitchDb<T>().Delete<T>();

        public IExpDelete<T> DeleteWithAttr<T>(T entity)
            => context.SwitchDb<T>().Delete(entity);

        public IExpDelete<T> DeleteWithAttr<T>(params T[] entities)
            => context.SwitchDb<T>().Delete(entities);

        public ISingleScopedExpressionContext CreateMainDbScoped()
        {
            return context.CreateScoped("MainDb");
        }

        public IExpSelect<TTemp1, TTemp2> FromTemp<TTemp1, TTemp2>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2)
        {
            var builder = SelectBuilder.GetSelectBuilder();
            HandleFromTemp(builder, temp1, temp2);
            return new SelectProvider2<TTemp1, TTemp2>(context, builder);
        }

        public IExpSelect<TTemp1, TTemp2, TTemp3> FromTemp<TTemp1, TTemp2, TTemp3>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3)
        {
            var builder = SelectBuilder.GetSelectBuilder();
            HandleFromTemp(builder, temp1, temp2, temp3);
            return new SelectProvider3<TTemp1, TTemp2, TTemp3>(context, builder);
        }

        public IExpSelect<TTemp1, TTemp2, TTemp3, TTemp4> FromTemp<TTemp1, TTemp2, TTemp3, TTemp4>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3, IExpTemp<TTemp2> temp4)
        {
            var builder = SelectBuilder.GetSelectBuilder();
            HandleFromTemp(builder, temp1, temp2, temp3, temp4);
            return new SelectProvider4<TTemp1, TTemp2, TTemp3, TTemp4>(context, builder);
        }

        public IExpSelect<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5> FromTemp<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(IExpTemp<TTemp1> temp1, IExpTemp<TTemp2> temp2, IExpTemp<TTemp2> temp3, IExpTemp<TTemp2> temp4, IExpTemp<TTemp2> temp5)
        {
            var builder = SelectBuilder.GetSelectBuilder();
            HandleFromTemp(builder, temp1, temp2, temp3, temp4, temp5);
            return new SelectProvider5<TTemp1, TTemp2, TTemp3, TTemp4, TTemp5>(context, builder);
        }
    }

    private static void HandleFromTemp(SelectBuilder sqlbuilder, params IExpTemp[] temps)
    {
        foreach (var temp in temps)
        {
            sqlbuilder.HandleTempsRecursion(temp.SqlBuilder);
            sqlbuilder.SelectedTables.Add(temp.ResultTable);
        }
    }
}

public static class ScopedExpressionContextExtensions
{
    extension(IScopedExpressionContext context)
    {
        private IScopedExpressionContext SwitchDb<T>()
        {
            var table = TableContext.GetTableInfo<T>();
            if (table.TargetDatabase != null)
            {
                return context.SwitchDatabase(table.TargetDatabase);
            }

            throw new LightOrmException("实体上没有设置DatabaseKey，无法自动切换数据库");
        }

        public IExpSelect<T> SelectWithAttr<
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            T>()
            => context.SwitchDb<T>().Select<T>();

        public IExpInsert<T> InsertWithAttr<T>(T entity)
            => context.SwitchDb<T>().Insert(entity);

        public IExpInsert<T> InsertWithAttr<T>(params T[] entities)
            => context.SwitchDb<T>().Insert(entities);

        public IExpUpdate<T> UpdateWithAttr<T>()
            => context.SwitchDb<T>().Update<T>();

        public IExpUpdate<T> UpdateWithAttr<T>(T entity)
            => context.SwitchDb<T>().Update(entity);

        public IExpUpdate<T> UpdateWithAttr<T>(params T[] entities)
            => context.SwitchDb<T>().Update(entities);

        public IExpDelete<T> DeleteWithAttr<T>()
            => context.SwitchDb<T>().Delete<T>();

        public IExpDelete<T> DeleteWithAttr<T>(T entity)
            => context.SwitchDb<T>().Delete(entity);

        public IExpDelete<T> DeleteWithAttr<T>(params T[] entities)
            => context.SwitchDb<T>().Delete(entities);
    }
}