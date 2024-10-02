using LightORM;
using LightORM.Interfaces;

namespace DatabaseUtils.Services
{
    public abstract class DbOperatorBase
    {
        protected readonly string ConnectionString;
        protected readonly IExpressionContext context;

        public DbOperatorBase(IExpressionContext context, string connStr)
        {
            ConnectionString = connStr;
            this.context = context;
        }

        protected abstract IDatabaseProvider GetConnectInfo();

    }
}
