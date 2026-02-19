namespace LightORM.DbEntity.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class SelectExtensionAttribute : Attribute
    {
        public int ArgumentCount { get; set; }
    }
}
