namespace LightORM;

internal sealed class TransientContext :ExpressionCoreSqlBase, ITransientContext
{
    private readonly DatabaseConnection connection;
    public override SqlAdo Ado => new(connection);
    //internal ExpressionSqlOptions Options { get; }
    //private readonly
    internal TransientContext(DatabaseConnection connection, ExpressionSqlOptions options)
        :base(options)
    {
        //Options = options;
        this.connection = connection;
    }

//    public MultipleResult QueryMultiple(params IExpSelect[] selects)
//        => ExpressionCoreSqlContextMethodImpl.QueryMultiple(Ado, selects);

//    public Task<MultipleResult> QueryMultipleAsync(IExpSelect[] selects, CancellationToken cancellationToken = default)
//        => ExpressionCoreSqlContextMethodImpl.QueryMultipleAsync(Ado, selects, cancellationToken);

//    public IExpSelect<T> Select<
//#if NET8_0_OR_GREATER
//       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
//#endif
//    T>() => ExpressionCoreSqlContextMethodImpl.Select<T>(this);

//    #region insert

//    public IExpInsert<T> Insert<T>() => ExpressionCoreSqlContextMethodImpl.Insert<T>(Ado);

//    public IExpInsert<T> Insert<T>(params T[] entities) => ExpressionCoreSqlContextMethodImpl.Insert(Ado, entities);

//    #endregion

//    #region update

//    public IExpUpdate<T> Update<T>() => ExpressionCoreSqlContextMethodImpl.Update<T>(Ado);

//    public IExpUpdate<T> Update<T>(params T[] entities) => ExpressionCoreSqlContextMethodImpl.Update(Ado, entities);

//    #endregion

//    #region delete

//    public IExpDelete<T> Delete<T>() => ExpressionCoreSqlContextMethodImpl.Delete<T>(Ado);

//    public IExpDelete<T> Delete<T>(params T[] entities) => ExpressionCoreSqlContextMethodImpl.Delete(Ado, entities);

//    #endregion

//    #region 数据库表操作

//    public string? CreateTableSql<T>(Action<TableOptions>? action = null)
//    {
//        var ado = Ado;
//        return ExpressionCoreSqlContextMethodImpl.InternalCreateTableSql<T>(ado.Provider, action);
//    }

//    public async Task<bool> CreateTableAsync<T>(Action<TableOptions>? action = null, CancellationToken cancellationToken = default)
//    {
//        var ado = Ado;
//        return await ExpressionCoreSqlContextMethodImpl.InternalCreateTableAsync<T>(ado, Options, action, cancellationToken);
//    }

//    public async Task<IList<DbStruct.ReadedTable>> GetTablesAsync()
//    {
//        var ado = Ado;
//        return await ExpressionCoreSqlContextMethodImpl.InternalGetTablesAsync(ado, Options);
//    }

//    public async Task<DbStruct.ReadedTable> GetTableStructAsync(DbStruct.ReadedTable table)
//    {
//        var ado = Ado;
//        return await ExpressionCoreSqlContextMethodImpl.InternalTableStructAsync(table, ado, Options);
//    }

//    public async Task<bool> DropTableAsync<T>(CancellationToken cancellationToken = default)
//    {
//        var ado = Ado;
//        var t = TableContext.GetTableInfo<T>();
//        return await ExpressionCoreSqlContextMethodImpl.InternalDropTableAsync(ado, t.TableName, cancellationToken);
//    }

//    #endregion
}