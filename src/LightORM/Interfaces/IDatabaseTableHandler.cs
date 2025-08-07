using LightORM.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Interfaces;

public interface IDatabaseTableHandler
{
    IEnumerable<string> GenerateDbTable<T>();
    void SaveDbTableStruct();
    //string ConvertToDbType(DbColumn type);
}
