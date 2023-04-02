using System.Collections.Generic;

namespace MDbContext.DbStruct
{
    internal struct DbTable
    {
        public string Name { get; set; }
        public IEnumerable<DbColumn> Columns { get; set; }
    }
}
