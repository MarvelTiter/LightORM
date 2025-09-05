using System.Threading;
namespace LightORM.ExpressionSql;

internal sealed partial class ExpressionCoreSql(ExpressionSqlOptions option) : ExpressionCoreSqlBase, IExpressionContext
{
    public override ExpressionSqlOptions Options { get; } = option;
    internal SqlExecutorProvider executorProvider = new(option);
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public override ISqlExecutor Ado => executorProvider.GetSqlExecutor(Options.DefaultDbKey);

    public ITransientExpressionContext SwitchDatabase(string key)
    {
        var ado = executorProvider.GetSqlExecutor(key);
        return TransientExpressionCoreSql.Create(key, ado);
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
