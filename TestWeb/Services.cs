using LightORM;
using TestWeb.Models;

namespace TestWeb
{
    public class Services1(IExpressionContext context, ILogger<Services1> logger)
    {
        public string Test()
        {
            return context.Id;
        }

        public Task Query()
        {
            var list = context.Select<Power>().ToList();
            foreach (var item in list)
            {
                logger.LogInformation("{PowerId} - {PowerName}", item.PowerId, item.PowerName);
            }
            return Task.CompletedTask;
        }
    }

    public class Services2(IExpressionContext context)
    {
        public string Test()
        {
            return context.Id;
        }
    }
}
