using Generators.Shared;
using Microsoft.CodeAnalysis;
using System;
using System.Drawing;
using System.Linq;

namespace LightOrmExtensionGenerator
{

    [Generator(LanguageNames.CSharp)]
    public class SelectProvidersGenerator : GeneratorBase
    {
        public override string FileName()
        {
            return "SelectProviderT3`16.g.cs";
        }
        public override string Namespace()
        {
            return "LightORM.Providers.Select";
        }
        public override string Handler(AttributeData data)
        {
            var count = (int)data.GetNamedValue("ArgumentCount")!;
            string argsStr = GetTypesString(count);

            string join = count < 16 ? $$"""

                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.InnerJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                
                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.LeftJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        JoinHandle<TJoin>(flatExp, ExpressionSql.TableLinkType.RightJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }

                """ : "";

            var selecteds = Enumerable.Range(1, count).Select(i => $"""
                        SqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T{i}>());
            """);

            var code = $$"""

internal sealed class SelectProvider{{count}}<{{argsStr}}> : SelectProvider0<IExpSelect<{{argsStr}}>, T1>, IExpSelect<{{argsStr}}>
{
    public SelectProvider{{count}}(ISqlExecutor executor, SelectBuilder? builder = null) 
        : base(executor, builder)
    {
        if (builder == null)
        {
            SqlBuilder = new SelectBuilder
            {
                DbType = DbType,
                IncludeContext = new(DbType)
            };
{{string.Join("\n", selecteds)}}
        }
    }

    public IExpGroupSelect<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<{{argsStr}}, TGroup>> exp)
    {
        return GroupByHandle<TGroup, TypeSet<{{argsStr}}>>(exp);
    }
    public IExpGroupSelect<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<TypeSet<{{argsStr}}>, TGroup>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return GroupByHandle<TGroup, TypeSet<{{argsStr}}>>(flatExp);
    }
    public IExpSelect<{{argsStr}}> OrderBy(Expression<Func<{{argsStr}}, object>> exp, bool asc = true)
    {
        return OrderByHandle(exp, asc);
    }
    public IExpSelect<{{argsStr}}> OrderBy(Expression<Func<TypeSet<{{argsStr}}>, object>> exp, bool asc = true)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return OrderByHandle(flatExp, asc);
    }
    public IExpSelect<{{argsStr}}> Where(Expression<Func<{{argsStr}}, bool>> exp)
    {
        return WhereHandle(exp);
    }
    public IExpSelect<{{argsStr}}> Where(Expression<Func<TypeSet<{{argsStr}}>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return WhereHandle(flatExp);
    }
{{join}}
    public IEnumerable<dynamic> ToDynamicList(Expression<Func<{{argsStr}}, object>> exp)
    {
        HandleResult(exp, null);
        return ToList<MapperRow>();
    }

    public IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToList<MapperRow>();
    }

    public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<{{argsStr}}, object>> exp)
    {
        HandleResult(exp, null);
        var list = await ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }

    public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        var list = await ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp)
    {
        HandleResult(exp, null);
        return ToList<TReturn>();
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp)
    {
        HandleResult(exp, null);
        return ToListAsync<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToListAsync<TReturn>();
    }

    public string ToSql(Expression<Func<{{argsStr}}, object>> exp)
    {
        HandleResult(exp, null);
        return ToSql();
    }
    
    public string ToSql(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToSql();
    }
}
""";
            return code;
        }
    }
}
