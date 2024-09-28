using Generators.Shared;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace LightOrmExtensionGenerator
{
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
