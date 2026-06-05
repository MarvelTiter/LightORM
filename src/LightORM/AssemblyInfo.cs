global using static LightORM.AssemblyControl.DebugControl;
global using static LightORM.AssemblyControl.AOTControl;
using LightORM.DbEntity.Attributes;
using LightORM.Extension;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("InjectTest")]
[assembly: InternalsVisibleTo("TestProject1")]
[assembly: InternalsVisibleTo("LightORMTest")]
[assembly: InternalsVisibleTo("LightORMTest.PostgreSQL")]
[assembly: SelectExtension(ArgumentCount = 2)]
[assembly: SelectExtension(ArgumentCount = 3)]
[assembly: SelectExtension(ArgumentCount = 4)]
[assembly: SelectExtension(ArgumentCount = 5)]
[assembly: SelectExtension(ArgumentCount = 6)]
[assembly: SelectExtension(ArgumentCount = 7)]
[assembly: SelectExtension(ArgumentCount = 8)]
[assembly: SelectExtension(ArgumentCount = 9)]
[assembly: SelectExtension(ArgumentCount = 10)]
[assembly: SelectExtension(ArgumentCount = 11)]
[assembly: SelectExtension(ArgumentCount = 12)]
[assembly: SelectExtension(ArgumentCount = 13)]
[assembly: SelectExtension(ArgumentCount = 14)]
[assembly: SelectExtension(ArgumentCount = 15)]
[assembly: SelectExtension(ArgumentCount = 16)]
[assembly: SelectExtension(ArgumentCount = 17)]

namespace LightORM.AssemblyControl
{
    internal static class DebugControl
    {
        public static readonly bool ShowExpressionResolveDebugInfo = false;
        public static readonly bool ShowSqlExecutorDebugInfo = true;
        public static readonly bool ShowExpressionHashCodeDebugInfo = true;
    }

    internal static class AOTControl
    {
        public static bool AOTSupported = false;

        public static void EnsureReflectionAccess(Type type)
        {
#if NET8_0_OR_GREATER
            if (!RuntimeFeature.IsDynamicCodeSupported && !type.IsAnonymous())
            {
                if (!AOTSupported)
                    throw new NotSupportedException($"""
                        当前环境不支持动态代码生成，无法通过反射访问类型成员。 +
                        类型: {type.FullName}
                        解决方案：SetTableContext配置生成器生成的上下文。
                        匿名类型（如 Select/GroupBy 投影）不受此限制。
                        """);
            }
#endif
        }
    }
}