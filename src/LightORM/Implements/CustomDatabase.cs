using System.Text;

namespace LightORM.Implements;

public abstract class CustomDatabase : ICustomDatabase
{
    public abstract string Prefix { get; }
    public abstract string Emphasis { get; }
    public ISqlMethodResolver MethodResolver { get; }
    protected readonly HashSet<string> keyWorks;
    protected CustomDatabase(ISqlMethodResolver resolver)
    {
        MethodResolver = resolver;
        keyWorks = [.. SetKeyWords()];
    }

    public virtual IEnumerable<string> SetKeyWords()
    {
        return [];
    }

    public virtual void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        throw new NotSupportedException();
    }

    public virtual string ReturnIdentitySql()
    {
        throw new NotSupportedException();
    }

    public virtual string HandleBooleanValue(bool value)
    {
        return value ? "1" : "0";
    }

    public bool IsKeyWork(string keyWork)
    {
        throw new NotImplementedException();
    }

    public virtual string DeleteTemplate => throw new NotImplementedException();
}
