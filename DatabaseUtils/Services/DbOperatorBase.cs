using LightORM;

namespace DatabaseUtils.Services
{
    public abstract class DbOperatorBase
    {
        protected readonly string ConnectionString;

        public DbOperatorBase(string connStr)
        {
            ConnectionString = connStr;
        }
        public ISqlExecutor Db => CreateDbContext();

        protected abstract ISqlExecutor CreateDbContext();

    }
}
