using System.Threading;
namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql(ExpressionSqlOptions option) : ExpressionCoreSqlBase, IExpressionContext
{
    public ExpressionSqlOptions Options { get; } = option;
    internal SqlExecutorProvider executorProvider = new(option);
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override ISqlExecutor Ado => executorProvider.GetSqlExecutor(Options.DefaultDbKey);

    public ITransientExpressionContext SwitchDatabase(string key)
    {
        var ado = executorProvider.GetSqlExecutor(key);
        return TransientExpressionCoreSql.Create(key, ado);
    }

    public string? CreateTableSql<T>(Action<TableGenerateOption>? action = null)
    {
        var ado = Ado;
        try
        {
            return string.Join(Environment.NewLine, GenerateDbTable<T>(ado, Options, action));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CreateTableAsync<T>(Action<TableGenerateOption>? action = null, CancellationToken cancellationToken = default)
    {
        var ado = Ado;
        try
        {
            var sqls = GenerateDbTable<T>(ado, Options, action);
            if (!sqls.Any())
            {
                return false;
            }
            ado.BeginTransaction();
            foreach (var s in sqls)
            {
                await ado.ExecuteNonQueryAsync(s, cancellationToken: cancellationToken);
            }
            await ado.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            await ado.RollbackTransactionAsync(cancellationToken);
            return false;
        }
    }
    private static IEnumerable<string> GenerateDbTable<T>(ISqlExecutor ado, ExpressionSqlOptions option, Action<TableGenerateOption>? action = null)
    {
        if (ado.Database.TableHandler is not null)
        {
            var o = option.TableGenOption;
            if (action != null)
            {
                o = (TableGenerateOption)o.Clone();
                action(o);
            }
            var handler = ado.Database.TableHandler.Invoke(o);
            var tableSql = handler.GenerateDbTable<T>();
            return tableSql;
        }
        return [];
    }
    public IExpSelect Select(string tableName) => throw new NotImplementedException();//new SelectProvider0(tableName, Ado);


    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                System.Diagnostics.Debug.WriteLine($"释放ExpressionCoreSql：{DateTime.Now}");
                executorProvider.Dispose();
            }

            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
