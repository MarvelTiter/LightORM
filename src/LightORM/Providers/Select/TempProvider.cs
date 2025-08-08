namespace LightORM.Providers.Select
{
    internal class TempProvider<TTemp> : IExpTemp<TTemp>
    {
        public string Id { get; } = $"{Guid.NewGuid():N}";
        public TableInfo ResultTable { get; }

        public SelectBuilder SqlBuilder { get; }

        public TempProvider(string name, SelectBuilder builder)
        {
            SqlBuilder = builder;
            SqlBuilder.IsTemp = true;
            SqlBuilder.TempName = name;
            ResultTable = TableInfo.Create<TTemp>();
            ResultTable.TableEntityInfo.CustomName = name;
            ResultTable.TableEntityInfo.IsTempTable = true;
        }
    }
}
