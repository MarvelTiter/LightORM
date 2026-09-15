using LightORM.Interfaces;

namespace LightORM.Builder;

/// <summary>
/// 适配器可选能力(能力接口)探测助手。
/// 穿透 <see cref="ScopedDatabaseAdapter"/> 装饰器直达真实实现，
/// 便于调用点用 <c>TryGet&lt;TCapability&gt;(database, out var cap)</c> 判断方言是否具备某特化能力。
/// 不支持的方言探测失败即返回 false，调用点据此给出明确的"不支持"提示。
/// </summary>
internal static class AdapterCapability
{
    public static bool TryGet<TCapability>(IDatabaseAdapter adapter, out TCapability? capability)
        where TCapability : class
    {
        for (var current = adapter; current is not null; current = current is ScopedDatabaseAdapter scope ? scope.Inner : null)
        {
            if (current is TCapability typed)
            {
                capability = typed;
                return true;
            }
        }
        capability = null;
        return false;
    }
}
