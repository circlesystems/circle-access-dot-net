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
        
        [Route("OnScan")]
        async public Task<IActionResult> OnScan(string sessionID, string userID)
        {
            List<string> registeredEmails = new List<string>()
            {
                "gene.allen@circlesecurity.ai",
                "mickeymouserules@toonmail.com",
                "bugsbunnywascrossing@lolmail.net",
                "pinkpantherpartytime@laughtermail.org",
                "scoobysnacklover@cartoonmail.biz",
                "donaldduckquackattack@quackymail.co",
                "fredflintstoneroxx@prehistoricmail.tv",
                "shaggysandwichlover@munchmail.cc",
                "spongebobsquarepantsdance@bikinibottommail.info",
                "wilecoyoteacmefails@clumsymail.me",
                "daffyducklooneytunes@funnymail.dev"
            };

            CircleAccessSession caSession = new CircleAccessSession(_caSettings.AppKey, _caSettings.ReadKey, _caSettings.WriteKey);
            CircleAccessStatus cas = await caSession.GetScanningEmailAsync(sessionID, userID, registeredEmails);
            if (cas.Success)
            {
                if (string.IsNullOrEmpty(cas.Email))  // we didn't error, but we didn't find a match either
                    return View("OnScan", "No matching email was found");

                return View("OnScan", cas.Email);
            }
            else
            {
                return View("OnScan", cas.ErrorMsg);
            }
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