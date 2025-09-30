using IncidentManagementsSystemNOSQL.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace IncidentManagementsSystemNOSQL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoDatabase _db;

        public HomeController(ILogger<HomeController> logger, IMongoDatabase db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        [HttpGet("/health/mongo")]
        public async Task<IActionResult> MongoHealth()
        {
            await _db.RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }");
            var collections = await _db.ListCollectionNames().ToListAsync();
            return Ok(new { ok = true, collections });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}