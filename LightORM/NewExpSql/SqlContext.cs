using MDbContext.NewExpSql.SqlFragment;
using System.Collections.Generic;
using System.Text;

namespace MDbContext.NewExpSql
{
    internal class SqlContext : ISqlContext
    {
        List<BaseFragment> fragments;
        Dictionary<string, object> parameters;

        public SqlContext()
        {
            fragments = new List<BaseFragment>();
            parameters = new Dictionary<string, object>();
        }

        public void AddFragment<F>(F fragment) where F : BaseFragment
        {
            fragments.Add(fragment);
        }

        public string BuildSql(out Dictionary<string, object> keyValues)
        {
            StringBuilder sql = new StringBuilder();
            foreach (var item in fragments)
            {
                foreach (var kv in item.SqlParameters)
                {
                    if (parameters.ContainsKey(kv.Key))
                    {
                        if (parameters[kv.Key]?.ToString() == kv.Value?.ToString())
                            continue;
                    }
                    parameters.Add(kv.Key, kv.Value);
                }
                sql.AppendLine(item.ToString());
            }
            keyValues = parameters;
            return sql.ToString();
        }
    }
}
