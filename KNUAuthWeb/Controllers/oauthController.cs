using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;
using System.Configuration;
using MySqlX.XDevAPI;
using System.Security.Policy;
using System.Net.NetworkInformation;
using System.Web;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace KNUAuthWeb.Controllers
{
    //[Route("/kauth/[controller]")]
    public class oauthController : Controller
    {

        // GET: /xxx/Register
        public ActionResult register()
        {
            try
            {
                Response.Cookies.Delete("client_id");
                Response.Cookies.Delete("responseUrl");
                Response.Cookies.Delete("state");
                Response.Cookies.Delete("scope");
            }
            catch { }
            return View();
        }
        
        // POST: /xxx/Register
        [HttpPost]
        public ActionResult register(User model)
        {
            
            if (ModelState.IsValid)
            {
                Connector connector = new Connector();
                connector.database = "test";
                connector.port = 3306;
                connector.user = "root";
                connector.password = "Qw123456";
                connector.server = "localhost";
                // Проверка, не занят ли уже такой Username
                bool existingUser = MySQL.firstOfDefault(connector,model.Username); 
                if (!existingUser)
                {
                    ModelState.AddModelError("Username", $"This username is already taken. {model.Username}");
                    return View(model);
                }

                // Добавление нового пользователя в базу данных
                MySQL.addUser(connector,model.Username, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password+model.Username))).Replace("-", "").ToLower());

                // После успешной регистрации можно перенаправить пользователя на другую страницу
                return RedirectToAction("login");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult login()
        {
            string scope = Request.Cookies["scope"];
            //string scope = HttpContext.Request.Query["scope"];
            if (scope != null)
            {
                TempData["scope"] = scope;
                TempData["login.text"] = "Авторизувати";
            }
            else
            {
                TempData["login.text"] = "Увійти";
                TempData["scope"] = null;
            }
            return View();
        }
        [HttpPost]
        public IActionResult login(User model)
        {
            Connector connector = new Connector();
            connector.database = "test";
            connector.port = 3306;
            connector.user = "root";
            connector.password = "Qw123456";
            connector.server = "localhost";
            int userId = MySQL.checkAuth(connector, model.Username, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password + model.Username))).Replace("-", "").ToLower());
            if (userId == 0)
            {
                ModelState.AddModelError("Password", $"");
                TempData["Error"] = $"Password or username incorrect!";
                return View(model);
            }
            string scope = "", responseUrl = "", state = ""; int client_id = 0;
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(90)
            };
            Response.Cookies.Append("user_token", MySQL.getActualToken(connector,userId), cookieOptions);
            try
            {
                scope = Request.Cookies["scope"];
                client_id = int.Parse(Request.Cookies["client_id"]);
                responseUrl = Request.Cookies["responseUrl"];
                state = Request.Cookies["state"];
            }
            catch
            {
                return RedirectToAction("Index","Home");
            }
            string code = BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Username + scope + client_id + DateTime.Now.Ticks))).Replace("-", "").ToLower().Substring(0, 16);
            if (!MySQL.addCode(connector, code, 300, scope, client_id, userId)) { return RedirectToAction("Home"); }
            if (state != null)
            { return Redirect(responseUrl + $"?code={code}&state={state}"); }
            else
            { return Redirect(responseUrl + $"?code={code}"); }
        }
        [HttpGet]
        public IActionResult OK()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OK(User model)
        {
                return RedirectToAction("OK");
        }
        [HttpGet]
        public IActionResult Authorize()
        {
            Connector connector = new Connector();
            connector.database = "test";
            connector.port = 3306;
            connector.user = "root";
            connector.password = "Qw123456";
            connector.server = "localhost";
            string rType = ""; string rUrl = ""; string scope = ""; string state = ""; int cID = 0;
            if (HttpContext.Request.Query.ContainsKey("response_type")) { rType = HttpContext.Request.Query["response_type"]; if (rType == null) { return StatusCode(500, "Error: response_type is Empty!"); } }
            if (HttpContext.Request.Query.ContainsKey("client_id")) { cID = int.Parse(HttpContext.Request.Query["client_id"]); if (cID == 0) { return StatusCode(500, "Error: client_id is Empty!"); } }
            if (HttpContext.Request.Query.ContainsKey("redirect_uri")) { rUrl = HttpContext.Request.Query["redirect_uri"]; if (rUrl == null) { return StatusCode(500, "Error: redirect_uri is Empty!"); } }
            if (HttpContext.Request.Query.ContainsKey("scope")) { scope = HttpContext.Request.Query["scope"]; if (scope == null) { return StatusCode(500, "Error: scope is Empty!"); } }
            if (HttpContext.Request.Query.ContainsKey("state")) { state = HttpContext.Request.Query["state"]; }
            bool check = MySQL.checkClient(connector, cID);
            if (!check) { return RedirectToAction("Home"); }
            TempData["scope"] = scope;
            TempData["login.text"] = "Авторизувати";
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5)
            };
            Response.Cookies.Append("client_id", $"{cID}", cookieOptions);
            Response.Cookies.Append("responseUrl", $"{rUrl}", cookieOptions);
            Response.Cookies.Append("state", $"{state}", cookieOptions);
            Response.Cookies.Append("scope", $"{scope}", cookieOptions);
            return RedirectToAction("login"/*, new { client_id = cID, responseUrl=rUrl, state = state, scope=scope}*/);

        }
    }
}
