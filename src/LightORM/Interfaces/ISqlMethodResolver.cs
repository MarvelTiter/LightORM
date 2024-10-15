namespace LightORM.Interfaces
{
    public interface ISqlMethodResolver
    {
        internal void Resolve(IExpressionResolver resolver, MethodCallExpression expression);
        void AddOrUpdateMethod(string methodName, Action<IExpressionResolver, MethodCallExpression> methodResolver);
        //void Count(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Sum(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Avg(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Max(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Min(IExpressionResolver resolver, MethodCallExpression methodCall);

        //#region 字符串
        //void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Contains(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Substring(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void Trim(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void TrimStart(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void TrimEnd(IExpressionResolver resolver, MethodCallExpression methodCall);
        //#endregion

        //#region include用到的方法
        //void Where(IExpressionResolver resolver, MethodCallExpression methodCall);
        //void When(IExpressionResolver resolver, MethodCallExpression methodCall);
        //#endregion
    }
}
