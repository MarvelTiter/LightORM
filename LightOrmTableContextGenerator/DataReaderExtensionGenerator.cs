using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightOrmTableContextGenerator;

[Generator(LanguageNames.CSharp)]
public class DataReaderExtensionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            GeneratorHelpers.LightTableAttributeFullName
            , static (node, _) => node is ClassDeclarationSyntax
            , static (ctx, _) => ctx);

        context.RegisterSourceOutput(source, static (context, source) =>
        {
            var targetSymbol = (INamedTypeSymbol)source.TargetSymbol;
            var exClass = CreateExtensionClass(targetSymbol);
            var file = CodeFile.New($"{targetSymbol.FormatFileName()}.DataRecord.g.cs")
            .AddMembers(NamespaceBuilder.Default.Namespace(targetSymbol.ContainingNamespace.ToDisplayString()).AddMembers(exClass))
            .AddUsings(targetSymbol.GetTargetUsings());
            context.AddSource(file);
        });
    }

    private static ClassBuilder CreateExtensionClass(INamedTypeSymbol @class)
    {
        var cb = ClassBuilder.Default.ClassName($"{@class.FormatClassName()}DataReaderExtensions").Modifiers("public static");

        var exMethod = MethodBuilder.Default.MethodName($"To{@class.MetadataName}ListAsync")
            .Modifiers("public static")
            .ReturnType($"global::System.Threading.Tasks.Task<global::System.Collections.Generic.IEnumerable<{@class.MetadataName}>>")
            .AddParameter("this global::System.Data.IDataReader reader")
            .AddGeneratedCodeAttribute(typeof(DataReaderExtensionGenerator));

        exMethod.AddBody("throw new Exception()");

        cb.AddMembers(exMethod);

        return cb;
    }
}
