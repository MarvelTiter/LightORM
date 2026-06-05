using Generators.Shared;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace LightOrmExtensionGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class TypeSetGenerator : GeneratorBase
    {
        public override bool Enable(AttributeData data)
        {
            return true;
        }
        public override string FileName()
        {
            return "TypeSetT2`17.g.cs";
        }

        public override string Handler(AttributeData data)
        {
            var count = (int)data.GetNamedValue("ArgumentCount")!;
            var argsStr = TypeArgsWithAttribute(count);
            var ctorArgs = GetTypesString(count, i => $"T{i} t{i}", ", ");
            var properties = CreateProperties(count);

            var code = $$"""

                public class TypeSet<
                {{string.Join(",", argsStr)}}
                >({{ctorArgs}})
                {
                {{string.Join("", properties)}}
                }
                """;
            return code;

            static IEnumerable<string> TypeArgsWithAttribute(int count)
            {
                for (var i = 1; i <= count; i++)
                {
                    if (i == 1)
                    {
                        yield return $"""
                            //#if NET8_0_OR_GREATER
                            //    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]
                            //#endif
                            T{i}
                            """;
                    }
                    else
                    {
                        yield return $"""

                            //#if NET8_0_OR_GREATER
                            //    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]
                            //#endif
                            T{i}
                            """;
                    }
                }
            }

            static IEnumerable<string> CreateProperties(int count)
            {
                for (var i = 1; i <= count; i++)
                {
                    if (i == 1)
                        yield return $$"""
                                    public T{{i}} Tb{{i}} { get; } = t{{i}};
                                """;
                    else
                        yield return $$"""

                                    public T{{i}} Tb{{i}} { get; } = t{{i}};
                                """;
                }
            }
        }
    }
}
