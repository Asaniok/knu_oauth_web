using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
        public IActionResult Index()
        {
            Connector connector = new Connector();
            connector.database = "test";
            connector.port = 3306;
            connector.user = "root";
            connector.password = "Qw123456";
            connector.server = "localhost";
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
