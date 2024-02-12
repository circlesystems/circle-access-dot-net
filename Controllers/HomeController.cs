using CircleAccessDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime;

namespace CircleAccessDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CircleAccessSettings _caSettings;

        public HomeController(ILogger<HomeController> logger, IOptions<CircleAccessSettings> caSettings)
        {
            _logger = logger;
            _caSettings = caSettings.Value;
        }

        public IActionResult Index()
        {
            return View("Index", _caSettings.LoginUrl);
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}