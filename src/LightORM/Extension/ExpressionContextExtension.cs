namespace LightORM;

public static class ExpressionContextExtension
{
    public static int BulkCopy(this IExpressionContext context, DataTable dataTable)
    {
        return context.Ado.Database.BulkCopy(dataTable);
    }
}

