using LightORM.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InjectTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IExpressionContext context;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IExpressionContext context)
        {
            _logger = logger;
            this.context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<object>> Get()
        {
            var a1 = await context.Select<Power>().ToListAsync();
            var a2 = await context.Select<Power>().ToListAsync();
            var a3 = await context.Select<Power>().ToListAsync();

            return a1.Concat(a2).Concat(a3);
        }

        [HttpGet("v2")]
        public async Task<IEnumerable<object>> Get2()
        {
            var a1 = await context.Select<Power>().ToListAsync();
            var a2 = await context.Select<Power>().ToListAsync();
            var a3 = await context.Select<Power>().ToListAsync();

            return a1.Concat(a2).Concat(a3);
        }
    }
}