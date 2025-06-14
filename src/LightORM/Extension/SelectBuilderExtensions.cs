﻿using LightORM.Builder;
using LightORM.Cache;
using LightORM.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Extension
{
    internal static class BuilderExtensions
    {
        public static void AddUnion(this SelectBuilder builder, SelectBuilder select, bool all)
        {
            select.IsUnion = true;
            select.UnionIndex = builder.Unions.Count;
            builder.Unions.Add(new(select, all));
            builder.HandleTempsRecursion(select);
        }

        public static void HandleTempsRecursion(this SelectBuilder main, SelectBuilder temp)
        {
            foreach (var item in temp.TempViews)
            {
                HandleTempsRecursion(main, item);
            }
            temp.TempViews.Clear();
            if (main.TempViews.Any(s => s.Id == temp.Id))
            {
                return;
            }
            if (temp.IsTemp)
            {
                main.TempViews.Add(temp);
            }
        }
    }
    //internal static class SelectBuilderExtensions
    //{
    //    static MethodInfo QueryMethod = typeof(SqlExecutorExtensions).GetMethods().Where(m => m.Name == "Query" && m.IsGenericMethod).FirstOrDefault()!;
    //    static MethodInfo ToList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!;
    //    public static void BindIncludeDatas(this SelectBuilder builder, ISqlExecutor executor, object data)
    //    {
    //        if (data is IEnumerable datas)
    //        {
    //            foreach (object item in datas)
    //            {
    //                foreach (IncludeInfo include in builder.Includes)
    //                {
    //                    var mainWhere = BuildMainWhereExpression(item, include.ParentWhereColumn!);
    //                    var includeBuilder = BuildSql(builder, include, mainWhere);
    //                    var sql = includeBuilder.ToSqlString();
    //                    var param = includeBuilder.DbParameters;
    //                    var typedQuery = QueryMethod.MakeGenericMethod(include.SelectedTable!.Type!);
    //                    var result = typedQuery.Invoke(null, [executor, sql, param, null, CommandType.Text]);
    //                    if (include.NavigateInfo!.IsMultiResult)
    //                    {
    //                        var tolist = ToList.MakeGenericMethod(include.SelectedTable!.Type!);
    //                        result = tolist.Invoke(null, [result]);
    //                    }
    //                    include.ParentNavigateColumn!.SetValue(item, result);
    //                    builder.BindIncludeDatas(executor, result);
    //                }
    //            }
    //        }

    //    }

    //    private static SelectBuilder BuildSql(SelectBuilder builder, IncludeInfo include, Expression mainWhere)
    //    {
    //        SelectBuilder selectSql = new SelectBuilder(builder.DbType);
    //        selectSql.SelectedTables.Add(include.SelectedTable!);
    //        selectSql.DbParameterStartIndex = include.ExpressionResolvedResult!.DbParameters?.Count ?? 0;
    //        selectSql.DbParameters.TryAddDictionary(include.ExpressionResolvedResult!.DbParameters);
    //        selectSql.Expressions.Add(new ExpressionInfo
    //        {
    //            Expression = BuildSelectAllExpression(include.SelectedTable!.Type!),
    //            ResolveOptions = SqlResolveOptions.Select,
    //        });

    //        selectSql.Expressions.Add(new ExpressionInfo
    //        {
    //            Expression = mainWhere,
    //            ResolveOptions = SqlResolveOptions.Where,
    //        });

    //        var mainNav = selectSql.MainTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == include.NavigateInfo!.MappingType).First().NavigateInfo!;
    //        var mainCol = selectSql.MainTable.GetColumnInfo(mainNav.MainName!);
    //        var parentTable = include.ParentTable!;
    //        if (include.NavigateInfo!.MappingType != null)
    //        {
    //            var mapTable = new TableInfo(include.NavigateInfo!.MappingType, selectSql.NextTableIndex);
    //            var subCol = mapTable.GetColumnInfo(mainNav.SubName!);
    //            selectSql.Joins.Add(new JoinInfo
    //            {
    //                EntityInfo = mapTable,
    //                JoinType = ExpressionSql.TableLinkType.LeftJoin,
    //                Where = $"( {selectSql.AttachEmphasis(selectSql.MainTable.Alias!)}.{selectSql.AttachEmphasis(mainCol.ColumnName)} = {selectSql.AttachEmphasis(mapTable.Alias!)}.{selectSql.AttachEmphasis(subCol.ColumnName)} )"
    //            });
    //            subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
    //            selectSql.Joins.Add(new JoinInfo
    //            {
    //                EntityInfo = parentTable,
    //                JoinType = ExpressionSql.TableLinkType.LeftJoin,
    //                Where = $"( {selectSql.AttachEmphasis(parentTable.Alias!)}.{selectSql.AttachEmphasis(include.ParentWhereColumn.ColumnName)} = {selectSql.AttachEmphasis(mapTable.Alias!)}.{selectSql.AttachEmphasis(subCol.ColumnName)} )"

    //            });
    //        }
    //        else
    //        {
    //            var subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
    //            selectSql.Joins.Add(new JoinInfo
    //            {
    //                EntityInfo = parentTable,
    //                JoinType = ExpressionSql.TableLinkType.LeftJoin,
    //                Where = $"( {selectSql.AttachEmphasis(parentTable.Alias!)}.{selectSql.AttachEmphasis(include.ParentWhereColumn.ColumnName)} = {selectSql.AttachEmphasis(selectSql.MainTable.Alias!)}.{selectSql.AttachEmphasis(subCol.ColumnName)} )"

    //            });
    //        }

    //        if (include.ExpressionResolvedResult!.NavigateDeep > 0)
    //        {
    //            selectSql.Where.Add(include.ExpressionResolvedResult!.SqlString!);
    //        }

    //        return selectSql;
    //    }

    //    private static LambdaExpression BuildSelectAllExpression(Type type)
    //    {
    //        var p = Expression.Parameter(type, "t");
    //        var lambda = Expression.Lambda(p, p);
    //        return lambda;
    //    }
    //    private static LambdaExpression BuildMainWhereExpression(object item, ITableColumnInfo col)
    //    {
    //        var p = Expression.Parameter(col.TableType);
    //        var equal = Expression.Equal(Expression.Property(p, col.PropertyName), Expression.Constant(col.GetValue(item)));
    //        return Expression.Lambda(equal, p);
    //    }
    //}
}
