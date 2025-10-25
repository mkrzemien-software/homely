using Homely.API.Entities;
using Homely.API.Examples;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers
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
        private readonly IUnitOfWork _unitOfWork;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IUnitOfWork  unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> Post()
        {
            var household = new HouseholdEntity
            {
                Name = "EMPEKA !"
            };
            
            await _unitOfWork.Households.AddAsync(household);

            await _unitOfWork.SaveChangesAsync();

            return Ok(household);
        }
    }
}
