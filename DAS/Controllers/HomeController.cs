using System.Diagnostics;
using DAS.Models;
using Microsoft.AspNetCore.Mvc;

namespace DAS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Accessed the Index page.");
            return View(); 
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Accessed the Privacy page.");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error occurred with Request ID: {RequestId}", errorId);
            return View(new ErrorViewModel { RequestId = errorId });
        }
    }
}
