namespace TestProject1;

public class TestInitContext : DbInitialContext
{
    public override void Initialized(IDbInitial db)
    {
        db.CreateTable<Product>();
    }
}
