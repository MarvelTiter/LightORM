using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace LightOrmExtensionGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class SqlMethodResolverCtorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var target = context.SyntaxProvider.CreateSyntaxProvider(
                static(node, _) => node is ClassDeclarationSyntax @class && @class.Identifier.Text == "BaseSqlMethodResolver",
                static(ctx, _) => ctx
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
                    if (item.Name == "Resolve" || item.MethodKind == MethodKind.Constructor) continue;
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
    [Generator(LanguageNames.CSharp)]
    public class SelectExtensionGenerator : GeneratorBase
    {
        public override string FileName()
        {
            return "ExpSelectExtensionT3`16.g.cs";
        }

        public override string Handler(AttributeData data)
        {
            var count = (int)data.GetNamedValue("ArgumentCount")!;
            string argsStr = GetTypesString(count);
            var types = Enumerable.Range(1, count).Select(i => $"typeof(T{i})");
            var code = $$"""

public static partial class SelectExtension
{
    public static IExpSelect<{{argsStr}}> Select<{{argsStr}}>(this IExpressionContext instance)
    {
        var key = GetDbKey({{string.Join(", ", types)}});
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider{{count}}<{{argsStr}}>(instance.Ado);
    }
    public static IExpSelect<{{argsStr}}> InnerJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.InnerJoin);
        return select;
    }
    
    public static IExpSelect<{{argsStr}}> InnerLeft<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.LeftJoin);
        return select;
    }
    
    public static IExpSelect<{{argsStr}}> InnerRight<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.RightJoin);
        return select;
    }

    public static IExpSelect<{{argsStr}}> InnerJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.InnerJoin);
        return select;
    }
    
    public static IExpSelect<{{argsStr}}> InnerLeft<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.LeftJoin);
        return select;
    }
    
    public static IExpSelect<{{argsStr}}> InnerRight<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.RightJoin);
        return select;
    }
}

""";
            return code;
        }
    }
}
