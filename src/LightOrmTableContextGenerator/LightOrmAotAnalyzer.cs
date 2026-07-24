using Generators.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LightOrmTableContextGenerator;

[Generator(LanguageNames.CSharp)]
public class LightOrmAotAnalyzer : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var nodes = context.SyntaxProvider.CreateSyntaxProvider(
             (node, _) => node is InvocationExpressionSyntax invocation, (ctx, token) =>
         {
             var invocation = (InvocationExpressionSyntax)ctx.Node;
             var semanticModel = ctx.SemanticModel;

             // 使用上面的检测方法
             if (IsAdoExtensionMethodCall(invocation, semanticModel, token, out var symbol))
             {
                 return (invocation, symbol);
             }
             return (null, null)!;
         }).Where(s => s.invocation != null).Collect();
        context.RegisterSourceOutput(context.CompilationProvider.Combine(nodes), static (context, source) =>
        {
            var (compilation, nodes) = source;
            var aa = compilation.Assembly.GetAttributes().Where(a => a.AttributeClass?.Name == "AssemblyMetadataAttribute").ToArray();
            foreach (var item in aa)
            {
                if (item.GetConstructorValue(0, out var name) && item.GetConstructorValue(1, out var value))
                {
                    if (name is string s && (s == "IsAotCompatible" || s == "IsTrimmable")
                     && value is string v && v == "True")
                    {
                        // 裁剪, 发出警告
                        foreach (var node in nodes)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                                id: "TCGA00001",
                                title: "AOT不兼容",
                                messageFormat: $"{node!.symbol!.MetadataName}不支持AOT, 考虑先调用Execute<T>",
                                category: typeof(TableContextGenerator).FullName!,
                                defaultSeverity: DiagnosticSeverity.Warning,
                                isEnabledByDefault: true), node.invocation.GetLocation()));
                        }
                        return;
                    }
                }
            }

        });
    }

    public static bool IsAdoExtensionMethodCall(InvocationExpressionSyntax invocation
        , SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol? symbol)
    {
        symbol = null;
        var i = invocation.ToFullString();

        if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol)
            return false;
        symbol = methodSymbol;
        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
            return false;

        // extension块的扩展方法 LightORM.SqlExecutorExtensions.extension(LightORM.ISqlExecutor)

        // 常规的扩展方法 LightORM.SqlExecutorExtensions
        var containingTypeString = containingType.ToDisplayString();

        var isSqlExcutorExtensionMethod = containingTypeString == "LightORM.SqlExecutorExtensions";
        var isExecuteMethod = methodSymbol.Name == "Execute";

        return isSqlExcutorExtensionMethod && !isExecuteMethod
            && containingType.IsStatic == true
            && methodSymbol.IsExtensionMethod == true;
    }
}
