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

public static partial class SelectExtensions
{
    public static IExpSelect<{{argsStr}}> Select<{{argsStr}}>(this IExpressionContext instance)
    {
        var key = GetDbKey({{string.Join(", ", types)}});
        if (key != null)
            instance.SwitchDatabase(key);
        return new SelectProvider{{count}}<{{argsStr}}>(instance.Ado);
    }
    /// <summary>
    /// 条件Where
    /// </summary>
    public static IExpSelect<{{argsStr}}> WhereIf<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, bool condition, Expression<Func<{{argsStr}}, bool>> exp)
    {
        if (condition)
        {
            select.Where(exp);
        }
        return select;
    }
    /// <summary>
    /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
    /// </summary>
    public static IExpSelect<{{argsStr}}> InnerJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.InnerJoin);
        return select;
    }
    
    /// <summary>
    /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
    /// </summary>
    public static IExpSelect<{{argsStr}}> LeftJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.LeftJoin);
        return select;
    }
    
    /// <summary>
    /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
    /// </summary>
    public static IExpSelect<{{argsStr}}> RightJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, bool>> on)
    {
        select.JoinHandle(on, TableLinkType.RightJoin);
        return select;
    }

    public static IEnumerable<dynamic> ToDynamicList<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, object>> exp)
    {
        select.HandleResult(exp, null);
        return select.InternalToList<MapperRow>();
    }
    
    public static async Task<IList<dynamic>> ToDynamicListAsync<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, object>> exp, CancellationToken cancellationToken = default)
    {
        select.HandleResult(exp, null);
        var list = await select.InternalToListAsync<MapperRow>(cancellationToken);
        return [..list.Cast<dynamic>()];
    }
    
    public static DataTable ToDataTable<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, object>> exp)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }
    
    public static Task<DataTable> ToDataTableAsync<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<{{argsStr}}, object>> exp, CancellationToken cancellationToken = default)
    {
        select.HandleResult(exp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters, cancellationToken: cancellationToken);
    }

    #region TypeSet

    /// <summary>
    /// 条件Where
    /// </summary>
    public static IExpSelect<{{argsStr}}> WhereIf<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, bool condition, Expression<Func<TypeSet<{{argsStr}}>, bool>> exp)
    {
        if (condition)
        {
            select.Where(exp);
        }
        return select;
    }

    /// <summary>
    /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
    /// </summary>
    public static IExpSelect<{{argsStr}}> InnerJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, bool>> on)
    {
        var flatExp = FlatTypeSet.Default.Flat(on)!;
        select.JoinHandle(flatExp, TableLinkType.InnerJoin);
        return select;
    }
    
    /// <summary>
    /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
    /// </summary>
    public static IExpSelect<{{argsStr}}> LeftJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, bool>> on)
    {
        var flatExp = FlatTypeSet.Default.Flat(on)!;
        select.JoinHandle(flatExp, TableLinkType.LeftJoin);
        return select;
    }
    
    /// <summary>
    /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
    /// </summary>
    public static IExpSelect<{{argsStr}}> RightJoin<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, bool>> on)
    {
        var flatExp = FlatTypeSet.Default.Flat(on)!;
        select.JoinHandle(flatExp, TableLinkType.RightJoin);
        return select;
    }

    public static IEnumerable<dynamic> ToDynamicList<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        return select.InternalToList<MapperRow>();
    }
    
    public static async Task<IList<dynamic>> ToDynamicListAsync<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, object>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var list = await select.InternalToListAsync<MapperRow>(cancellationToken);
        return [..list.Cast<dynamic>()];
    }
    
    public static DataTable ToDataTable<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, object>> exp)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTable(sql, parameters);
    }
    
    public static Task<DataTable> ToDataTableAsync<{{argsStr}}>(this IExpSelect<{{argsStr}}> select, Expression<Func<TypeSet<{{argsStr}}>, object>> exp, CancellationToken cancellationToken = default)
    {
        var flatExp = FlatTypeSet.Default.Flat(exp)!;
        select.HandleResult(flatExp, null);
        var sql = select.SqlBuilder.ToSqlString();
        var parameters = select.SqlBuilder.DbParameters;
        return select.Executor.ExecuteDataTableAsync(sql, parameters, cancellationToken: cancellationToken);
    }

    #endregion
}

""";
            return code;
        }
    }
}
