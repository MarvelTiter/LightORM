using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models
{
    internal class SimpleColumn
    {
        public SimpleColumn(bool isPrimaryKey, string columnName, string parameterName, string propName, object? val)
        {
            IsPrimaryKey = isPrimaryKey;
            ColumnName = columnName;
            ParameterName = parameterName;
            PropName = propName;
            Value = val;
        }
        public bool IsPrimaryKey { get; set; }
        public string ColumnName { get; set; }
        public string ParameterName { get; set; }
        public string PropName { get; set; }
        public object? Value { get; set; }
    }
    internal class BatchSqlInfo
    {
        public BatchSqlInfo(List<List<SimpleColumn>> parameters)
        {
            Parameters = parameters;
        }
        public string? Sql { get; set; }
        public List<List<SimpleColumn>> Parameters { get; set; }
    }
}
