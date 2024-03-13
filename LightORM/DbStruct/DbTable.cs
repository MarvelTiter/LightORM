using System.Collections.Generic;

namespace LightORM.DbStruct;

internal struct DbTable
{
    public string Name { get; set; }
    public IEnumerable<DbIndex> Indexs { get; set; }
    public IEnumerable<DbColumn> Columns { get; set; }
}
