using LightORM.Extension;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace LightORM.Implements;

internal abstract class CustomDatabaseAdapter : IDatabaseAdapter
{
    public abstract string Prefix { get; }
    public abstract string Emphasis { get; }
    public ISqlMethodResolver MethodResolver { get; }
    //public IJsonColumnHandler JsonHandler { get; }
    public bool UseIdentifierQuote { get; set; } = true;

    private readonly HashSet<string> keyWorks = new(StringComparer.OrdinalIgnoreCase)
    {
        // 数据查询（DQL/DML）
        "SELECT", "FROM", "WHERE", "INSERT", "UPDATE", "DELETE",
        "INTO", "VALUES", "SET", "MERGE",

        // 数据连接与组合
        "JOIN", "INNER", "OUTER", "LEFT", "RIGHT", "FULL", "ON",
        "UNION", "ALL",

        // 数据过滤与分组
        "AND", "OR", "NOT", "IN", "BETWEEN", "LIKE", "IS",
        "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "DISTINCT",

        // 数据定义与结构（DDL）
        "CREATE", "ALTER", "DROP", "TABLE", "DATABASE", "SCHEMA",
        "INDEX", "VIEW", "COLUMN", "CONSTRAINT", "PRIMARY", "FOREIGN",
        "KEY", "UNIQUE", "DEFAULT",

        // 数据类型和函数
        "NULL", "TRUE", "FALSE", "COUNT", "SUM", "AVG", "MAX", "MIN",

        // 事务控制
        "COMMIT", "ROLLBACK", "TRANSACTION",

        // 权限管理
        "GRANT", "REVOKE",

        // 需要特别警惕的"高危"词
        "USER", "DATE", "TIME", "TIMESTAMP",
        "COMMENT", "TYPE", "STATUS", "SESSION", "VALUE"
    };

    protected CustomDatabaseAdapter(ISqlMethodResolver resolver)
    {
        MethodResolver = resolver;
        //JsonHandler = GetJsonHandler();
        // ReSharper disable once VirtualMemberCallInConstructor
        foreach (var keyWord in AddAdditionalKeyWords())
        {
            keyWorks.Add(keyWord);
        }
    }

    //protected virtual IJsonColumnHandler GetJsonHandler()
    //{
    //    throw new NotSupportedException();
    //}

    protected virtual IEnumerable<string> AddAdditionalKeyWords()
    {
        return [];
    }

    public virtual void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        throw new NotSupportedException();
    }

    public virtual void ReturnIdentitySql(StringBuilder sql)
    {
        throw new NotSupportedException();
    }

    public virtual void HandleBooleanValue(StringBuilder sql, bool value)
    {
        sql.Append(FormatBooleanValue(value));
    }

    public virtual string FormatBooleanValue(bool value)
    {
        return value ? "1" : "0";
    }

    public virtual string FormatDateTimeValue(DateTime value)
    {
        throw new NotImplementedException();
    }

    public abstract void HandleDateValue(StringBuilder sql, DateTime dateTime);

    public virtual string HandleBooleanValueForBulkCopy(bool value)
    {
        return FormatBooleanValue(value);
    }

    public bool IsKeyWord(string keyWork)
    {
        return keyWorks.Contains(keyWork);
    }

    public void AddKeyWord(IEnumerable<string> keyworks)
    {
        this.keyWorks.UnionWith(keyworks);
    }

    public virtual string HandleMultipleQuerySql(string[] sqls, Dictionary<string, object> parameters)
    {
        return string.Join(";", sqls);
    }

    public virtual string RewriteParameterReferences(string sql, string prefix)
    {
        // 将 @param 重写为 @q0_param
        return Regex.Replace(sql, @$"{Prefix}(\w+)", m => $"{Prefix}{prefix}_{m.Groups[1].Value}");
    }

    public virtual string DeleteTemplate => throw new NotImplementedException();

    bool? IDatabaseAdapter.QuoteIdentifiers { get; set; }

    public virtual void HandleJsonColumn(JsonColumnContext context)
    {
        throw new NotSupportedException();
    }

    public virtual void HandleJsonParameter(JsonColumnParameterContext context) { }

    public virtual void DbCommandInit(DbCommand dbCommand) { }

    public virtual void HandleInsertOrUpdate(UpsertContext context)
    {
        throw new NotImplementedException();
    }

    public virtual void HandleBatchInsert(BatchActionContext context)
    {
        var batchs = context.Batchs;
        var builder = context.Builder;
        var insertColumns = context.TargetColumns;
        var database = context.ScopedAdapter;
        foreach (var item in batchs)
        {
            using var _ = StringBuilderPool.Get(out var sb);
            PreHandleInsertBuilder(sb);
            for (int i = 0; i < item.Parameters.Count; i++)
            {
                List<SimpleColumn>? dic = item.Parameters[i];
                if (i > 0)
                {
                    sb.Append(',');
                    sb.AppendLine();
                }
                sb.Append('(');
                foreach (var c in dic)
                {
                    sb.Append(this.GetValueExpression(c));
                    sb.Append(',');
                }
                sb.RemoveLast(1);
                sb.Append(')');
            }
            builder.HandleSqlParameters(sb, this);
            item.Sql = sb.ToString();
        }


        void PreHandleInsertBuilder(StringBuilder sb)
        {
            sb.Append("INSERT INTO ");
            //sb.Append(GetTableName(database, MainTable, false));
            sb.AppendTableName(database, builder.MainTable, false).AppendLine();
            sb.Append('(');
            foreach (var item in insertColumns)
            {
                sb.AppendEmphasis(item.ColumnName, this);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.Append(')');
            sb.AppendLine();
            sb.AppendLine("VALUES");
        }
    }

    public virtual void HandleBatchUpdate(BatchActionContext context)
    {
        var batchs = context.Batchs;
        var builder = context.Builder;
        var updateColumns = context.TargetColumns;
        var database = context.ScopedAdapter;
        foreach (var batch in batchs)
        {
            // 每一个BatchSqInfo就是每批次更新的数据量
            //StringBuilder sb = new("UPDATE ");
            using var _ = StringBuilderPool.Get(out var sb);
            sb.Append("UPDATE ");
            //sb.Append(GetTableName(database, MainTable, false));
            sb.AppendTableName(database, builder.MainTable, false);
            sb.Append(" SET ");
            for (int i = 0; i < updateColumns.Length; i++)
            {
                ITableColumnInfo? col = updateColumns[i];
                if (col.IsPrimaryKey) continue;
                //sb.Append($"\n{database.AttachEmphasis(col.ColumnName)} = CASE ");
                sb.AppendEmphasis(col.ColumnName, database);
                sb.AppendLine(" = CASE");
                // 每一条记录的参数数量
                for (var rowIndex = 0; rowIndex < batch.Parameters.Count; rowIndex++)
                {
                    var rowDatas = batch.Parameters[rowIndex];
                    var currentCol = rowDatas.First(r => r.PropName == col.PropertyName);
                    if (currentCol.IsVersion)
                    {
                        var newVersion = SqlBuilder.VersionPlus(currentCol.Value);
                        var newCol = currentCol with { ParameterName = $"{currentCol.ParameterName}_n", Value = newVersion, IsVersion = false };
                        rowDatas.Add(newCol);
                        currentCol = newCol;
                    }
                    bool first = true;
                    sb.Append("  WHEN ");
                    foreach (var item in rowDatas.Where(r => r.IsPrimaryKey || r.IsVersion))
                    {
                        if (!first) sb.Append(" AND ");
                        first = false;
                        sb.AppendEmphasis(item.ColumnName, database);
                        sb.Append(" = ");
                        sb.WithPrefix(item.ParameterName, database);
                    }
                    sb.Append(" THEN ");
                    sb.AppendLine(database.GetValueExpression(currentCol));
                }

                sb.Append("END, ");
            }

            sb.RemoveLast(2);

            var pValues = batch.Parameters.SelectMany(rowDatas => rowDatas.Where(r => r.IsPrimaryKey | r.IsVersion)).GroupBy(c => c.ColumnName).ToList();
            if (pValues.Count == 0 && builder.Where.Count == 0)
            {
                throw new LightOrmException($"类型{builder.MainTable.Type}, 没有主键并且缺失Where条件");
            }
            sb.AppendLine();
            sb.Append("WHERE ");
            for (int k = 0; k < pValues.Count; k++)
            {
                IGrouping<string, SimpleColumn>? item = pValues[k];
                if (k > 0)
                {
                    sb.AppendLine();
                    sb.Append("AND ");
                }
                sb.Append('(');
                sb.AppendEmphasis(item.Key, database);
                sb.Append(" IN (");
                foreach (var i in item)
                {
                    sb.WithPrefix(i.ParameterName, database);
                    sb.Append(',');
                }
                sb.RemoveLast(1);
                sb.Append("))");
            }
            if (builder.Where.Count > 0)
            {
                //if (pValues.Count == 0)
                for (int i = 0; i < builder.Where.Count; i++)
                {
                    if (i > 0 || pValues.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append("AND ");
                    }
                    sb.Append(builder.Where[i]);
                }
            }
            builder.HandleSqlParameters(sb, database);
            batch.Sql = sb.ToString();
        }
    }

    public virtual void HandleBatchDelete<T>(BatchActionContext<DeleteBuilder<T>> context)
    {
        var batchs = context.Batchs;
        var builder = context.Builder;
        var keyColumns = context.TargetColumns;
        var database = context.ScopedAdapter;

        foreach (var batch in batchs)
        {
            using var _ = StringBuilderPool.Get(out var sb);
            sb.Append("DELETE FROM ");
            //sb.AppendLine(GetTableName(database, MainTable, false));
            sb.AppendTableName(database, builder.MainTable, false).AppendLine();
            sb.Append("WHERE ");
            if (builder.TargetObjects.Length == 0)
            {
                sb.Append("1=0");
                batch.Sql = sb.ToString();
                break;
            }
            sb.Append('(');
            for (int rowIndex = 0; rowIndex < batch.Parameters.Count; rowIndex++)
            {
                List<SimpleColumn>? row = batch.Parameters[rowIndex];
                if (keyColumns.Length > 1)
                {
                    sb.Append('(');
                    for (var i = 0; i < row.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(" AND ");
                        }
                        sb.AppendEmphasis(row[i].ColumnName, database);
                        sb.Append(" = ");
                        sb.WithPrefix(row[i].ParameterName, database);
                    }
                    sb.Append(')');
                    if (rowIndex < batch.Parameters.Count - 1)
                        sb.Append(" OR ");
                }
                else
                {
                    if (rowIndex == 0)
                    {
                        // 这里直接访问索引0是安全的，因为进入到else分支的话，说明row.Count == 1, 而row.Count是跟前面的columns的数量是一致的
                        sb.AppendEmphasis(row[0].ColumnName, database);
                        sb.Append(" IN (");

                    }
                    sb.WithPrefix(row[0].ParameterName, database);
                    if (rowIndex < batch.Parameters.Count - 1)
                        sb.Append(',');
                    else
                        sb.Append(')');
                }
            }
            sb.Append(')');

            foreach (var w in builder.Where)
            {
                sb.AppendLine();
                sb.Append("AND ");
                sb.Append(w);
            }
            builder.HandleSqlParameters(sb, database);
            batch.Sql = sb.ToString();
        }
    }
}