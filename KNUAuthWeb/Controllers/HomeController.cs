using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KNUAuthWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult Index()
        {
            Connector connector = new Connector();
            connector.database = _configuration["database"];
            connector.port = int.Parse(_configuration["port"]);
            connector.user = _configuration["user"];
            connector.password = _configuration["password"];
            connector.server = _configuration["server"];
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            try
            {
                Response.Cookies.Delete("client_id");
                Response.Cookies.Delete("responseUrl");
                Response.Cookies.Delete("state");
                Response.Cookies.Delete("scope");
            }
            catch { }
            try { 
                string token = Request.Cookies["user_token"];
                if(token != null) {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                        return View();
                    }
                }
                  } catch { }
            return View();

        }

        [HttpPost]
        public IActionResult index()
        {
            try
            {
                Response.Cookies.Delete("client_id");
                Response.Cookies.Delete("responseUrl");
                Response.Cookies.Delete("state");
                Response.Cookies.Delete("scope");
            }
            catch {}
            try { Response.Cookies.Delete("user_token"); } catch { }
            return View();

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
