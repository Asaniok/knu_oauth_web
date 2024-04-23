using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace KNUAuthWeb.Controllers
{
    public class me : Controller
    {
        private readonly IConfiguration _configuration;

        public me(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Connector getConnector()
        {
            Connector connector = new Connector();
            connector.database = _configuration["database"];
            connector.port = int.Parse(_configuration["port"]);
            connector.user = _configuration["user"];
            connector.password = _configuration["password"];
            connector.server = _configuration["server"];
            return connector;
        }
        public ActionResult profile()
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            try
            {
                Response.Cookies.Delete("client_id");
                Response.Cookies.Delete("responseUrl");
                Response.Cookies.Delete("state");
                Response.Cookies.Delete("scope");
            }
            catch { }
            try
            {
                string token = Request.Cookies["user_token"];
                if (token != null)
                {
                    TempData["viewprofile"] = "viewprofile";
                    dbUser user = MySQL.getUserByToken(connector, token);
                    if (user != null)
                    {
                        @TempData["username"] = user.user;
                        @TempData["id"] = user.id;
                        @TempData["email"] = user.email;
                        @TempData["surname"] = user.surname;
                        @TempData["firstname"] = user.firstname;
                        @TempData["lastname"] = user.lastname;
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("login", "oauth");
                    }
                }
                else
                {
                    
                }
            }
            catch { }
            

            return View();
        }
        
        // POST: /xxx/Register
        [HttpPost]
        public ActionResult profile(User model)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            try
            {
                string token = HttpContext.Request.Query["oauth_token"];
                string method = HttpContext.Request.Query["method"];
                if (token != null&method=="getInfo")
                {
                    dbUser user = MySQL.getUserByToken(connector, token, "getInfo");
                    if (user != null)
                    {
                        return Ok(JsonSerializer.Serialize(user));
                    }
                    else
                    {
                        return StatusCode(500, "oauth_token incorrect or expired, try to use refresh_token!");
                    }
                }
                else
                {
                    return StatusCode(500, "oauth_token or method empty!");
                }
            }
            catch { }
            return View(model);
        }
    }
}
