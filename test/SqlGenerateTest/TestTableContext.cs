using LightORM;

namespace SqlGenerateTest;

[LightORMTableContext]
internal partial class TestTableContext
{
    public void H(Type type)
    {
        if (type == typeof(int) || type.IsAssignableFrom(typeof(int)))
        {

        }
    }
}