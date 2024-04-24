using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace KNUAuthWeb.Controllers
{
    public class adminController : Controller
    {
        [HttpGet]
        public IActionResult index(adminModel model, string login, string email, string s, string f, string l)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            try
            {
                string token = Request.Cookies["user_token"];
                if (token != null)
                {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                        if (MySQL.checkUserAdmin(connector, token))
                            @TempData["admin"] = "1";
                        else
                        {
                            @TempData["admin"] = null;
                            return RedirectToAction("Index","Home");
                        }
                            
                    }
                }
            }
            catch { }
            if (login == null & email == null & s == null & f == null & l == null)
            {
                adminModel.Users = MySQL.adminGetUsers(connector, 10);
            }
            else
            {
                adminModel.Users = MySQL.adminGetUserByFilter(connector, 10, login, email, s, f, l);
            }
            return View();
        }
        [HttpPost]
        public IActionResult index(User model)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            if (Request.Cookies["user_token"] == null) { return RedirectToAction("Index", "Home"); }
            string scope = Request.Cookies["scope"], state = Request.Cookies["state"], client_id = Request.Cookies["client_id"], responseUrl = Request.Cookies["responseUrl"];
            string code = BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Username + scope + client_id + DateTime.Now.Ticks))).Replace("-", "").ToLower().Substring(0, 16);
            if (!MySQL.addCode(connector, code, 300, scope, int.Parse(client_id), MySQL.getUserByToken(connector, Request.Cookies["user_token"]).id)) { return RedirectToAction("Home"); }
            Response.Cookies.Delete("client_id");
            Response.Cookies.Delete("responseUrl");
            Response.Cookies.Delete("state");
            Response.Cookies.Delete("scope");
            if (state != null)
            { return Redirect(responseUrl + $"?code={code}&state={state}"); }
            else
            { return Redirect(responseUrl + $"?code={code}"); }
        }
        private readonly IConfiguration _configuration;

        public adminController(IConfiguration configuration)
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
    }
}
