using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using KNUOAuthApi.Controllers;
using System.Text.RegularExpressions;

namespace KNUAuthWeb.Controllers
{
    public class oauthController : Controller
    {
        private readonly IConfiguration _configuration;

        public oauthController(IConfiguration configuration)
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
        public ActionResult register()
        {
            Response.Cookies.Delete("client_id");
            Response.Cookies.Delete("responseUrl");
            Response.Cookies.Delete("state");
            Response.Cookies.Delete("scope");
            return View();
        }
        [HttpPost]
        public ActionResult register(User model)
        {
            if (ModelState.IsValid)
            {
                Connector connector = getConnector();
                dbUser newUser;
                if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
                if (model.Username.Length > 50)
                {
                    ModelState.AddModelError("Username", $"Max 50 знаків");
                    return View(model);
                }
                if(model.Password.Length > 128)
                {
                    ModelState.AddModelError("Password", $"Max 128 знаків");
                    return View(model);
                }
                if (Regex.IsMatch(model.Username, @"^[a-zA-Z0-9_]+$"))
                {
                    bool existingUser = MySQL.firstOfDefault(connector, model.Username);
                    if (!existingUser)
                    {
                        ModelState.AddModelError("Username", $"Цей логін вже зайнятий.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("Username", $"Допустимі лише a-z,A-Z,0-9 та _");
                    return View(model);
                }
                if (Regex.IsMatch(model.RestoreEmail, @"^[a-zA-Z0-9_.+-]+@(knu\.ua|gmail\.com)+$"))
                {
                    bool existingEmail = MySQL.checkEmailUnique(connector, model.RestoreEmail);
                    if (!existingEmail)
                    {
                        ModelState.AddModelError("RestoreEmail", $"Цей email вже використано, спробуйте інший.");
                        return View(model);
                    }
                }else if(!Regex.IsMatch(model.RestoreEmail, @"^[a-z0-9_.+-]+@(knu\.ua|gmail\.com)+$"))
                {
                    ModelState.AddModelError("RestoreEmail", $"Дупустимі символи a-z,0-9,. та _ доменів knu.ua та gmail.com");
                    return View(model);
                }
                if (!Regex.IsMatch(model.FirstName, @"^[А-ЯІЇЄ]{1}[а-яіїє']+$"))
                {
                    ModelState.AddModelError("FirstName", $"Допустимі лише А-Я,а-я");
                    return View(model);
                }
                if (!Regex.IsMatch(model.middlename, @"^[А-ЯІЇЄ]{1}[а-яіїє']+$"))
                {
                    ModelState.AddModelError("middlename", $"Допустимі лише А-Я,а-я");
                    return View(model);
                }
                if (!Regex.IsMatch(model.Surname, @"^[А-ЯІЇЄ]{1}[а-яіїє']+$"))
                {
                    ModelState.AddModelError("Surname", $"Допустимі лише А-Я,а-я");
                    return View(model);
                }
                newUser = new dbUser
                {
                    user = model.Username,
                    email = model.RestoreEmail,
                    surname = model.Surname,
                    firstname = model.FirstName,
                    middlename = model.middlename
                };
                MySQL.addUser(connector,newUser, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password+model.Username))).Replace("-", "").ToLower());
                return RedirectToAction("login");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult login()
        {

            string scope = Request.Cookies["scope"];
            if (scope != null & Request.Cookies["user_token"] == null)
            {
                TempData["viewprofile"] = null;
                TempData["scope"] = scope;
                TempData["login.text"] = "Авторизувати";
            }
            else if (Request.Cookies["user_token"]!=null & scope != null)
            {
                Response.Cookies.Delete("client_id");
                Response.Cookies.Delete("responseUrl");
                Response.Cookies.Delete("state");
                Response.Cookies.Delete("scope");
                TempData["login.text"] = "Увійти";
                TempData["viewprofile"] = null;
                TempData["scope"] = scope;
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
            TempData["Error"] = $"";
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            int userId = MySQL.checkAuth(connector, model.Username, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password + model.Username))).Replace("-", "").ToLower());
            if (userId == 0)
            {
                if(Request.Cookies["scope"] != null) { TempData["scope"] = Request.Cookies["scope"]; }
                ModelState.AddModelError("Password", $"Невірний логін або пароль.");
                return View(model);
            }
            string scope = "", responseUrl = "", state = ""; int client_id = 0;
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(int.Parse(_configuration["cookiesAuthExpTime"]))
            };
            string user_token = MySQL.getActualToken(connector, userId);
            Response.Cookies.Delete("user_token");
            if (user_token == "IE01")
            {
                string tokenNew = tokenController.genToken(userId+"");
                MySQL.AddTokenB(connector, tokenNew,userId, tokenController.genToken(userId + "1"),999999999,"bearer","admin");
                Response.Cookies.Append("user_token", tokenNew, cookieOptions);
            }
            else
            {

                Response.Cookies.Append("user_token", MySQL.getActualToken(connector, userId), cookieOptions);
            }
            
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
            return RedirectToAction("grant");
            //string code = BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Username + scope + client_id + DateTime.Now.Ticks))).Replace("-", "").ToLower().Substring(0, 16);
            //if (!MySQL.addCode(connector, code, 300, scope, client_id, userId)) { return RedirectToAction("Home"); }
            //if (state != null)
            //{ return Redirect(responseUrl + $"?code={code}&state={state}"); }
            //else
            //{ return Redirect(responseUrl + $"?code={code}"); }
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
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            Response.Cookies.Delete("client_id");
            Response.Cookies.Delete("responseUrl");
            Response.Cookies.Delete("state");
            Response.Cookies.Delete("scope");
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
                Expires = DateTime.Now.AddMinutes(int.Parse(_configuration["cookiesLoginExpTime"]))
            };
            if (Request.Cookies["user_token"] == null)
            {
                Response.Cookies.Append("client_id", $"{cID}", cookieOptions);
                Response.Cookies.Append("responseUrl", $"{rUrl}", cookieOptions);
                Response.Cookies.Append("state", $"{state}", cookieOptions);
                Response.Cookies.Append("scope", $"{scope}", cookieOptions);
                return RedirectToAction("login");
            }
            else
            {
                Response.Cookies.Append("client_id", $"{cID}", cookieOptions);
                Response.Cookies.Append("responseUrl", $"{rUrl}", cookieOptions);
                Response.Cookies.Append("state", $"{state}", cookieOptions);
                Response.Cookies.Append("scope", $"{scope}", cookieOptions);
                return RedirectToAction("grant");
            }
            

        }
        [HttpGet]
        public IActionResult grant()
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            string scope = Request.Cookies["scope"];
            TempData["viewprofile"] = "viewprofile";
            if (scope == "getInfo")
            {
                TempData["scope"] = "Буде надано доступ до ваших облікових даних: * Логіну, прізвище, ім'я, по-батькові, ваш унікальний ID";

            }
            else
            {
                TempData["scope"] = scope;
            }
            try
            {
                string token = Request.Cookies["user_token"];
                if (token != null)
                {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        TempData["Username"] = username;
                        TempData["viewprofile"] = "viewprofile";
                        return View();
                    }
                }
            }
            catch { }

            return View();
        }
        [HttpPost]
        public IActionResult grant(User model)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            if (Request.Cookies["user_token"]==null || Request.Cookies["responseUrl"] == null) { return RedirectToAction("Index","Home"); }
            string scope = Request.Cookies["scope"], state = Request.Cookies["state"], client_id = Request.Cookies["client_id"], responseUrl = Request.Cookies["responseUrl"];
            string code = BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Username + scope + client_id + DateTime.Now.Ticks))).Replace("-", "").ToLower().Substring(0, 16);
            if (!MySQL.addCode(connector, code, 300, scope, int.Parse(client_id), MySQL.getUserByToken(connector,Request.Cookies["user_token"]).id)) { return RedirectToAction("Home"); }
            Response.Cookies.Delete("client_id");
            Response.Cookies.Delete("responseUrl");
            Response.Cookies.Delete("state");
            Response.Cookies.Delete("scope");
            if (state != null)
            { return Redirect(responseUrl + $"?code={code}&state={state}"); }
            else
            { return Redirect(responseUrl + $"?code={code}"); }
        }
    }
}
