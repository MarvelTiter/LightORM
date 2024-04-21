using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DatabaseUtils.Models
{
    public class DatabaseTable
    {
        [NotNull] public string? TableName { get; set; }
        [NotNull] public IEnumerable<TableColumn>? Columns { get; set; }

        public bool IsSelected { get; set; }
    }
}
