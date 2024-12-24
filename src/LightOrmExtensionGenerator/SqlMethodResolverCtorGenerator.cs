using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LightOrmExtensionGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class SqlMethodResolverCtorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var target = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax @class && @class.Identifier.Text == "BaseSqlMethodResolver",
                static (ctx, _) => ctx
                );
            context.RegisterSourceOutput(target, static (source, context) =>
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;
                if (symbol == null)
                {
                    return;
                }
                var classBuilder = ClassBuilder.Default.ClassName("BaseSqlMethodResolver");
                var file = CodeFile.New("BaseSqlMethodResolver.Constructor.g.cs").AddMembers(NamespaceBuilder.Default.Namespace("LightORM.Implements").AddMembers(classBuilder));
                var methods = symbol.GetMethods();
                var ctorMethod = ConstructorBuilder.Default.MethodName("BaseSqlMethodResolver");
                foreach (var item in methods)
                {
                    if (item.Name == "Resolve" || item.Name == "AddOrUpdateMethod" || item.MethodKind == MethodKind.Constructor) continue;
                    if (!item.IsVirtual) continue;
                    ctorMethod.AddBody($"methods.Add(nameof({item.Name}), {item.Name});");
                }
                classBuilder.AddMembers(ctorMethod);
#if DEBUG
                var ss = file.ToString();
#endif
                source.AddSource(file);
            });
        }
    }
}
