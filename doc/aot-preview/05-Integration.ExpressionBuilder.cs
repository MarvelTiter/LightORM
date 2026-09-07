// =====================================================================
// 接入点改造：ExpressionBuilder.BuildDeserializer 改双轨
// 生成的 Deserializer 优先，Expression 方案作为回退（匿名类型 / Tuple / 基础类型）
// =====================================================================

using LightORM.Cache;
using LightORM.Interfaces;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using static LightORM.AssemblyControl.AOTControl;   // IsAOTRuntime

namespace LightORM.SqlExecutor;

internal static class ExpressionBuilderIntegration
{
    /// <summary>
    /// 生成器产物登记表（internal，对生成的 .g.cs 可见）。
    /// LightOrmTableContextGenerator 额外生成一份模块初始化器：
    /// <code>
    /// [ModuleInitializer]
    /// internal static void RegisterAll()
    /// {
    ///     Generated.Register(ProductDeserializer.Instance);
    ///     Generated.Register(UserFlatDeserializer.Instance);
    ///     ...
    /// }
    /// </code>
    /// 这样连 TableContext 都不用查，一次字典读取即命中。
    /// </summary>
    internal static class Generated
    {
        private static readonly ConcurrentDictionary<Type, Delegate> _map = new();

        public static void Register<T>(Func<System.Data.IDataReader, T> func)
            => _map[typeof(T)] = func;

        public static Func<System.Data.IDataReader, T>? Get<T>()
            => _map.TryGetValue(typeof(T), out var d) && d is Func<System.Data.IDataReader, T> f ? f : null;
    }

    /// <summary>
    /// 替换现有 ExpressionBuilder.BuildDeserializer&lt;T&gt; 的入口。
    /// </summary>
    public static Func<IDataReader, T> BuildDeserializer<T>(DbDataReader reader)
    {
        // ---- 轨道 1：源生成器产物（AOT 安全，零编译开销） ----
        var generated = Generated.Get<T>()
                        ?? (TableContext.StaticContext?.GetTableInfo(typeof(T)) as ITableEntityInfo<T>)?.DataReaderDeserializer;
        if (generated is not null)
        {
            return generated;
        }

        // ---- 轨道 2：回退到 Expression（匿名类型 / Tuple / 基础类型 / 未注册实体） ----
        if (ExpressionBuilder.IsAOTRuntime)
        {
            // AOT 下 Expression.Compile 不可用，且匿名类型会被裁剪。
            // 由 LightOrmAotAnalyzer 在编译期就报出来，这里是运行期兜底。
            throw new LightOrmException(
                $"类型 {typeof(T).FullName} 没有可用的反序列化器。" +
                "AOT 模式下请为实体标注 [LightTable] 并接入 LightORMTableContext，" +
                "匿名类型/Tuple/标量查询请改用具名 DTO。");
        }

        return ExpressionBuilder.BuildDeserializer<T>(reader);
    }
}
