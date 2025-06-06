using Generators.Shared;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LightOrmExtensionGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class SelectInterfacesGenerator : GeneratorBase
    {
        public override string FileName()
        {
            return $"IExpSelectT3`16.g.cs";
        }

        private IEnumerable<string> CreateWithTemp(int count)
        {
            for (int i = 1; i <= 5; i++)
            {
                if (i + count > 16)
                {
                    yield break;
                }
                string argsStr = GetTypesString(count);
                var tempType = string.Join(", ", Enumerable.Range(1, i).Select(i => $"TTemp{i}"));
                var tempUsed = string.Join(", ", Enumerable.Range(1, i).Select(i => $"temp{i}"));
                var tempPara = string.Join(", ", Enumerable.Range(1, i).Select(i => $"IExpTemp<TTemp{i}> temp{i}"));

                yield return $$"""
                        IExpSelect<{{argsStr}}, {{tempType}}> WithTempQuery<{{tempType}}>({{tempPara}});
                    """;
            }
        }

        public override string Handler(AttributeData data)
        {
            var count = (int)data.GetNamedValue("ArgumentCount")!;
            string argsStr = GetTypesString(count);

            string join = count < 16 ? $$"""

                    IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp);
                    IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp);
                    IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp);

                    IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where);
                    IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where);
                    IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where);

                """ : "";

            string typeSetJoin = count < 16 ? $$"""

                    IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp);
                    IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp);
                    IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp);

                    IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where);
                    IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where);
                    IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where);
                """ : "";
            var code = $$"""

public interface IExpSelect<{{argsStr}}> : IExpSelect0<IExpSelect<{{argsStr}}>, T1>
{
    IExpSelect<{{argsStr}}> OrderBy(Expression<Func<{{argsStr}}, object>> exp);
    IExpSelect<{{argsStr}}> OrderBy(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);

    IExpSelect<{{argsStr}}> OrderByDesc(Expression<Func<{{argsStr}}, object>> exp);

    IExpSelectGroup<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<{{argsStr}}, TGroup>> exp);

    IExpSelect<{{argsStr}}> Where(Expression<Func<{{argsStr}}, bool>> exp);
    IExpSelect<{{argsStr}}> Where(Expression<Func<TypeSet<{{argsStr}}>, bool>> exp);
{{join}}
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, object>> exp);

    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp, CancellationToken cancellationToken = default);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<{{argsStr}}, object>> exp, CancellationToken cancellationToken = default);

    /// <summary>
    /// 转换成<see cref="IExpSelect{T1}"/>
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpSelect<TTable> AsTable<TTable>(Expression<Func<{{argsStr}}, TTable>> exp);

    /// <summary>
    /// 转换成WITH查询，用于<see cref="WithTempQuery"/>
    /// </summary>
    /// <typeparam name="TTemp"></typeparam>
    /// <param name="name"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<{{argsStr}}, TTemp>> exp);

    string ToSql(Expression<Func<{{argsStr}}, object>> exp);
    
    #region with temp

{{string.Join("\n", CreateWithTemp(count))}}

    #endregion

    #region TypeSet
{{typeSetJoin}}
    IExpSelect<{{argsStr}}> OrderByDesc(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);
    IExpSelectGroup<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<TypeSet<{{argsStr}}>, TGroup>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp);
    IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);

    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp, CancellationToken cancellationToken = default);
    Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, object>> exp, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, object>> exp, CancellationToken cancellationToken = default);

    /// <summary>
    /// 转换成<see cref="IExpSelect{TTable}"/>
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="exp"></param>    
    /// <returns></returns>
    IExpSelect<TTable> AsTable<TTable>(Expression<Func<TypeSet<{{argsStr}}>, TTable>> exp);
    /// <summary>
    /// 转换成WITH查询，用于<see cref="WithTempQuery"/>
    /// </summary>
    /// <typeparam name="TTemp"></typeparam>
    /// <param name="name"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<TypeSet<{{argsStr}}>, TTemp>> exp);
    string ToSql(Expression<Func<TypeSet<{{argsStr}}>, object>> exp);
    
    #endregion
}
""";
            return code;
        }
    }
}
