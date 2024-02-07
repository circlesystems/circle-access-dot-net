using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime;

namespace CircleAccessDemo.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly CircleAccessSettings _caSettings;

        public AuthenticationController(ILogger<AuthenticationController> logger, IOptions<CircleAccessSettings> caSettings)
        {
            _logger = logger;
            _caSettings = caSettings.Value;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AuthScanCallback(string sessionID, string userID)
        {
            List<string> registeredEmails = new List<string>()
            {
                "shivam.singhal@circlesecurity.ai",
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

    }
}
