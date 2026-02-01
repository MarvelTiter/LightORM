using Generators.Shared;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
                        public IExpSelect<{{argsStr}}, {{tempType}}> WithTempQuery<{{tempType}}>({{tempPara}})
                        {
                            this.HandleTempQuery({{tempUsed}});
                            return new SelectProvider{{i + count}}<{{argsStr}}, {{tempType}}>(Executor, SqlBuilder);
                        }
                    """;
            }
        }

        public override string Handler(AttributeData data)
        {
            var count = (int)data.GetNamedValue("ArgumentCount")!;
            string argsStr = GetTypesString(count);

            string join = count < 16 ? $$"""
                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> OuterJoin<TJoin>(Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    
                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.InnerJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.LeftJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.RightJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<{{argsStr}}, TJoin, bool>> exp)
                    {
                        this.JoinHandle<TJoin>(exp, TableLinkType.OuterJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    
                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where)
                    {
                        this.JoinHandle<TJoin>(where, TableLinkType.InnerJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }

                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where)
                    {
                        this.JoinHandle<TJoin>(where, TableLinkType.LeftJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where)
                    {
                        this.JoinHandle<TJoin>(where, TableLinkType.RightJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<{{argsStr}}, TJoin, bool>> where)
                    {
                        this.JoinHandle<TJoin>(where, TableLinkType.OuterJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                """ : "";

            string typeSetJoin = count < 16 ? $$"""

                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> OuterJoin<TJoin>(Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    
                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(string tableName, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(string tableName, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(string tableName, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> OuterJoin<TJoin>(string tableName, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> exp)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(exp)!;
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin, overriddenTableName: tableName);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    
                    public IExpSelect<{{argsStr}}, TJoin> InnerJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(where);
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.InnerJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                
                    public IExpSelect<{{argsStr}}, TJoin> LeftJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(where);
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.LeftJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> RightJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(where);
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.RightJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                    public IExpSelect<{{argsStr}}, TJoin> OuterJoin<TJoin>(IExpSelect<TJoin> subQuery, Expression<Func<TypeSet<{{argsStr}}, TJoin>, bool>> where)
                    {
                        var flatExp = FlatTypeSet.Default.Flat(where);
                        this.JoinHandle<TJoin>(flatExp, TableLinkType.OuterJoin, subQuery);
                        return new SelectProvider{{count + 1}}<{{argsStr}}, TJoin>(Executor, SqlBuilder);
                    }
                """ : "";

            var selecteds = Enumerable.Range(1, count).Select(i => $"""
                        SqlBuilder.SelectedTables.Add(TableInfo.Create<T{i}>({i - 1}));
            """);

            var code = $$"""

internal sealed class SelectProvider{{count}}<{{argsStr}}> : SelectProvider0<IExpSelect<{{argsStr}}>, T1>, IExpSelect<{{argsStr}}>
{
    public SelectProvider{{count}}(ISqlExecutor executor, SelectBuilder? builder = null) 
        : base(executor, builder)
    {
        if (builder == null)
        {
            SqlBuilder = new SelectBuilder();
{{string.Join("\n", selecteds)}}
        }
    }

    public IExpSelectGroup<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<{{argsStr}}, TGroup>> exp)
    {
        return this.GroupByHandle<TGroup, TypeSet<{{argsStr}}>>(exp);
    }

    public IExpSelect<{{argsStr}}> OrderBy<TOrder>(Expression<Func<{{argsStr}}, TOrder>> exp)
    {
        this.OrderByHandle(exp, true);
        return this;
    }

    public IExpSelect<{{argsStr}}> OrderByDesc<TOrder>(Expression<Func<{{argsStr}}, TOrder>> exp)
    {
        this.OrderByHandle(exp, false);
        return this;
    }

    public IExpSelect<{{argsStr}}> Where(Expression<Func<{{argsStr}}, bool>> exp)
    {
        this.WhereHandle(exp);
        return this;
    }

{{join}}
        
    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp)
    {
        this.HandleResult(exp, null);
        return this.InternalToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<{{argsStr}}, object>> exp)
    {
        this.HandleResult(exp, null);
        return this.InternalToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<{{argsStr}}, object>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<{{argsStr}}, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<{{argsStr}}, object>> exp, CancellationToken cancellationToken = default)
    {
        this.HandleResult(exp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IExpSelect<TTable> AsTable<TTable>(Expression<Func<{{argsStr}}, TTable>> exp)
    {
        this.HandleResult(exp, null);
        return new SelectProvider1<TTable>(Executor, SqlBuilder);
    }
    public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<{{argsStr}}, TTemp>> exp)
    {
        this.HandleResult(exp, null);
        return new TempProvider<TTemp>(name, SqlBuilder);
    }

    public string ToSql(Expression<Func<{{argsStr}}, object>> exp)
    {
        this.HandleResult(exp, null);
        return ToSql();
    }
    
    #region with temp
{{string.Join("\n", CreateWithTemp(count))}}
    #endregion
    #region TypeSet

{{typeSetJoin}}

    public IExpSelectGroup<TGroup, TypeSet<{{argsStr}}>> GroupBy<TGroup>(Expression<Func<TypeSet<{{argsStr}}>, TGroup>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        return this.GroupByHandle<TGroup, TypeSet<{{argsStr}}>>(flatExp);
    }

    public IExpSelect<{{argsStr}}> OrderBy<TOrder>(Expression<Func<TypeSet<{{argsStr}}>, TOrder>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.OrderByHandle(flatExp, true);
        return this;
    }

    public IExpSelect<{{argsStr}}> OrderByDesc<TOrder>(Expression<Func<TypeSet<{{argsStr}}>, TOrder>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.OrderByHandle(flatExp, false);
        return this;
    }

    public IExpSelect<{{argsStr}}> Where(Expression<Func<TypeSet<{{argsStr}}>, bool>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.WhereHandle(flatExp);
        return this;
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

    public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToList<TReturn>();
    }

    public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, object>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToListAsync<TReturn>(cancellationToken);
    }

     public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, TReturn>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IAsyncEnumerable<TReturn> ToEnumerableAsync<TReturn>(Expression<Func<TypeSet<{{argsStr}}>, object>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return this.InternalToEnumerableAsync<TReturn>(cancellationToken);
    }

    public IExpSelect<TTable> AsTable<TTable>(Expression<Func<TypeSet<{{argsStr}}>, TTable>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp);
        this.HandleResult(flatExp, null);
        return new SelectProvider1<TTable>(Executor, SqlBuilder);
    }
    public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<TypeSet<{{argsStr}}>, TTemp>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return new TempProvider<TTemp>(name, SqlBuilder);
    }

    public string ToSql(Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        this.HandleResult(flatExp, null);
        return ToSql();
    }

    #endregion
}
""";
            return code;
        }
    }
}
