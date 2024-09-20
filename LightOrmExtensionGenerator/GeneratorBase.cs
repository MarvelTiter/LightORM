using Generators.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace LightOrmExtensionGenerator
{
    public abstract class GeneratorBase : IIncrementalGenerator
    {
        public const string TargetAttribute = "LightORM.DbEntity.Attributes.SelectExtensionAttribute";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var source = context.SyntaxProvider.ForAttributeWithMetadataName(
                TargetAttribute
                , static (node, _) => true
                , (ctx, _) => ctx);

            context.RegisterSourceOutput(source, RegisterOutput);
            void RegisterOutput(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
            {
                var target = (IAssemblySymbol)source.TargetSymbol;
                var attrs = target.GetAttributes(TargetAttribute);
                foreach (var item in attrs)
                {
                    var result = Handler(item);
                    context.AddSource(result.Item1, SourceText.From(result.Item2, Encoding.UTF8));
                }
            }
        }

        public abstract (string, string) Handler(AttributeData data);

        protected static string GetTypesString(int count)
        {
            var args = Enumerable.Range(1, count).Select(i => $"T{i}");
            var argsStr = string.Join(", ", args);
            return argsStr;
        }
    }
}
