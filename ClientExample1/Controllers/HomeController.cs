using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using static System.Formats.Asn1.AsnWriter;

namespace KNUAuthWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> IndexAsync(string code, string state)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5)
            }; 
            if (state == "asdddddwsdasd")
            {
                try
                {
                    HttpClient client = new HttpClient();
                    var httpContent = new StringContent("", Encoding.UTF8, "application/json");
                    string queryString = $"?code={code}&client_id=1&grant_type=authorization_code&scope=getInfo";
                    var response = await client.PostAsync($"https://hotducks.org/oauth/token{queryString}", httpContent);
                    //var response = await client.PostAsync($"http://localhost:5000/oauth/token{queryString}", httpContent);
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject jsonPost = JObject.Parse(jsonResponse);
                    string acces_token = (string)jsonPost["acces_token"];
                    //response = await client.PostAsync($"http://localhost:5000/me/profile?oauth_token={acces_token}&method=getInfo", httpContent);
                    response = await client.PostAsync($"https://hotducks.org/me/profile?oauth_token={acces_token}&method=getInfo", httpContent);
                    jsonResponse = await response.Content.ReadAsStringAsync();
                    jsonPost = JObject.Parse(jsonResponse);
                    string surname = (string)(jsonPost["surname"]);
                    string firstname = (string)(jsonPost["firstname"]);
                    TempData["user"] = surname + " "+firstname;
                }catch { return View(); }
            }

            return View();
        }

        [HttpPost]
        public IActionResult Index()
        {
            int client = 1;
            //return Redirect($"http://localhost:5000/oauth/authorize?response_type=code&client_id={client}&redirect_uri=http://localhost:5001&scope=getInfo&state=asdddddwsdasd");
            return Redirect($"https://hotducks.org/oauth/authorize?response_type=code&client_id={client}&redirect_uri=https://client.hotducks.org&scope=getInfo&state=asdddddwsdasd");
        }
    }
}
