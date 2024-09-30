using System.Collections.Generic;

namespace LightORM.DbStruct;

public struct DbTable
{
    public string Name { get; set; }
    public IEnumerable<DbIndex> Indexs { get; set; }
    public IEnumerable<DbColumn> Columns { get; set; }
}
