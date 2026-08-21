using LightORM.DbStruct;
using LightORM.Implements;
using System.Text;
using LightORM.Providers.Dameng.TableStructure;

namespace LightORM.Providers.Dameng;

public sealed class DamengTableHandler(DamengTableOptions tableOptions)
    : BaseDatabaseHandler<DamengTableWriter, DamengTableOptions>

{
    public override DamengTableOptions Options => tableOptions;
    public override string GetTablesSql()
    {
        throw new NotImplementedException();
    }

    public override string GetTableStructSql(string table)
    {
        throw new NotImplementedException();
    }

    public override bool ParseDataType(ReadedTableColumn column, out string type)
    {
        throw new NotImplementedException();
    }
}
