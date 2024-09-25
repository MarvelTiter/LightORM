using Generators.Shared;
using Microsoft.CodeAnalysis;
using System.Drawing;

namespace LightOrmExtensionGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class SelectInterfacesGenerator : GeneratorBase
    {
        public override string FileName()
        {
            return $"IExpSelectT3`16.g.cs";
        }

        public override string Handler(AttributeData data)
        {
            var count = (int)data.GetNamedValue("ArgumentCount")!;
            string argsStr = GetTypesString(count);
            string join = count < 16 ? $$"""

                    IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp);
                    IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp);
                    IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp);

                """ : "";
            var code = $$"""

public interface IExpSelect<{{argsStr}}> : IExpSelect0<IExpSelect<{{argsStr}}>, T1>
{
    IExpSelect<{{argsStr}}> OrderBy(Expression<Func<{{argsStr}}, object>> exp, bool asc = true);
    IExpSelect<{{argsStr}}> OrderBy(Expression<Func<TypeSet<{{argsStr}}>, object>> exp, bool asc = true);

    IExpGroupSelect<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<{{argsStr}}, TGroup>> exp);
    IExpGroupSelect<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<TypeSet<{{argsStr}}>, TGroup>> exp);

    IExpSelect<{{argsStr}}> Where(Expression<Func<{{argsStr}}, bool>> exp);
    IExpSelect<{{argsStr}}> Where(Expression<Func<TypeSet<{{argsStr}}>, bool>> exp);
{{join}}
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp);

    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp);

    IEnumerable<dynamic> ToDynamicList(Expression<Func<{{argsStr}}, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<{{argsStr}}, object>> exp);

    IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);
    Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);

    string ToSql(Expression<Func<{{argsStr}}, object>> exp);
    string ToSql(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);
}
""";
            return code;
        }
    }
}
