using LightORM.Interfaces.ExpSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Select
{
    internal class TempProvider<TTemp> : IExpTemp<TTemp>
    {
        public string Id { get; } = $"{Guid.NewGuid():N}";
        public ITableEntityInfo ResultTable { get; }

        public SelectBuilder SqlBuilder { get; }

        public TempProvider(string name, SelectBuilder builder)
        {
            SqlBuilder = builder;
            SqlBuilder.IsTemp = true;
            SqlBuilder.TempName = name;
            ResultTable = TableContext.GetTableInfo<TTemp>();
            ResultTable.CustomName = name;
            ResultTable.IsTempTable = true;
        }
    }
}
