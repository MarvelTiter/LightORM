using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.Performances;

internal class SelectBuilderPool
{
    private static readonly ConcurrentStack<SelectBuilder> pool = new();
    public static SelectBuilder Rent()
    {
        if (pool.TryPop(out var builder))
        {
            ResetBuilder(builder);
            return builder;
        }
        return new SelectBuilder();
    }

    public static void Return(SelectBuilder builder)
    {
        if (builder == null) return;

        // 清理资源但不释放对象
        ClearBuilder(builder);

        if (pool.Count < ExpressionSqlOptions.Instance.Value.InternalObjectPoolSize)
            pool.Push(builder);
    }

    private static void ResetBuilder(SelectBuilder builder)
    {
        // 清空列表但保留容量
        builder.SelectedTables.Clear();
        builder.Where.Clear();
        builder.Joins.Clear();
        builder.GroupBy.Clear();
        builder.OrderBy.Clear();
        builder.Having.Clear();
        builder.Includes?.Clear();
        builder.TempViews.Clear();
        builder.Unions.Clear();
        builder.DbParameters.Clear();
        builder.Where.Clear();
        builder.DbParameterInfos.Clear();

        // 重置其他字段
        builder.PageIndex = 0;
        builder.PageSize = 0;
        builder.Skip = 0;
        builder.Take = 0;
        builder.IsDistinct = false;
        builder.IsRollup = false;
        builder.SelectValue = "*";
        builder.Level = 0;
        builder.TableIndexFix = 0;
        builder.IsSubQuery = false;
        builder.IsTemp = false;
        builder.IsUnion = false;
        builder.TempName = null;
        builder.InsertInfo = null;
        builder.SubQuery = null;
        builder.AdditionalValue = null;
        builder.TargetObject = null;
        builder.IsParameterized = true;

        // 清空Expressions
        builder.Expressions.ExpressionInfos.Clear();
    }

    private static void ClearBuilder(SelectBuilder builder)
    {
        // 如果列表容量过大，重建以释放内存
        ShrinkListIfLarge(builder.Where);
        ShrinkListIfLarge(builder.GroupBy);
        ShrinkListIfLarge(builder.OrderBy);
        ShrinkListIfLarge(builder.Joins);
        // ... 其他列表
    }

    private static void ShrinkListIfLarge<T>(List<T> list)
    {
        if (list.Capacity > 64)  // 如果容量太大
            list = new List<T>(8);  // 重建为小容量
    }
}
