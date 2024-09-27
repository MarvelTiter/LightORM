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
                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.InnerJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }

                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.LeftJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        JoinHandle<TJoin>(exp, ExpressionSql.TableLinkType.RightJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                """ : "";

            string typeSetJoin = count < 16 ? $$"""

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
            SqlBuilder = new SelectBuilder(DbType);
{{string.Join("\n", selecteds)}}
        }
    }

    public IExpSelectGroup<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<{{argsStr}}, TGroup>> exp)
    {
        return GroupByHandle<TGroup, TypeSet<{{argsStr}}>>(exp);
    }

    public IExpSelect<{{argsStr}}> OrderBy(Expression<Func<{{argsStr}}, object>> exp)
    {
        return OrderByHandle(exp, true);
    }

    public IExpSelect<{{argsStr}}> OrderByDesc(Expression<Func<{{argsStr}}, object>> exp)
    {
        return OrderByHandle(exp, false);
    }

    public IExpSelect<{{argsStr}}> Where(Expression<Func<{{argsStr}}, bool>> exp)
    {
        return WhereHandle(exp);
    }

{{join}}

    public IEnumerable<dynamic> ToDynamicList(Expression<Func<{{argsStr}}, object>> exp)
    {
        HandleResult(exp, null);
        return ToList<MapperRow>();
    }

    public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<{{argsStr}}, object>> exp)
    {
        HandleResult(exp, null);
        var list = await ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp)
    {
        HandleResult(exp, null);
        return ToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp)
    {
        HandleResult(exp, null);
        return ToListAsync<TReturn>();
    }

    public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<{{argsStr}}, TTemp>> exp)
    {
        HandleResult(exp, null);
        SqlBuilder.Level += 1;
        SqlBuilder.IsSubQuery = true;
        var builder = new SelectBuilder(DbType)
        {
            SubQuery = SqlBuilder,
        };
        builder.SelectedTables.Add(TableContext.GetTableInfo<TTemp>());
        return new SelectProvider1<TTemp>(Executor, builder);
    }

    public string ToSql(Expression<Func<{{argsStr}}, object>> exp)
    {
        HandleResult(exp, null);
        return ToSql();
    }
    
    #region TypeSet

{{typeSetJoin}}

    public IExpSelectGroup<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<TypeSet<{{argsStr}}>, TGroup>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return GroupByHandle<TGroup, TypeSet<{{argsStr}}>>(flatExp);
    }

    public IExpSelect<{{argsStr}}> OrderBy(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return OrderByHandle(flatExp, true);
    }

    public IExpSelect<{{argsStr}}> OrderByDesc(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return OrderByHandle(flatExp, false);
    }

    public IExpSelect<{{argsStr}}> Where(Expression<Func<TypeSet<{{argsStr}}>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return WhereHandle(flatExp);
    }

    public IEnumerable<dynamic> ToDynamicList(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToList<MapperRow>();
    }

    public async Task<IList<dynamic>> ToDynamicListAsync(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        var list = await ToListAsync<MapperRow>();
        return list.Cast<dynamic>().ToList();
    }
    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToListAsync<TReturn>();
    }

    public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<TypeSet<{{argsStr}}>, TTemp>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        SqlBuilder.Level += 1;
        SqlBuilder.IsSubQuery = true;
        var builder = new SelectBuilder(DbType)
        {
            SubQuery = SqlBuilder,
        };
        builder.SelectedTables.Add(TableContext.GetTableInfo<TTemp>());
        return new SelectProvider1<TTemp>(Executor, builder);
    }

    public string ToSql(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        HandleResult(flatExp, null);
        return ToSql();
    }

    #endregion
}
""";
            return code;
        }
    }
}
