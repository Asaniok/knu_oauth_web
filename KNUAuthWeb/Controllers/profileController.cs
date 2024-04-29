using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using KNUOAuthApi;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KNUAuthWeb.Controllers
{
    public class profile : Controller
    {
        private readonly IConfiguration _configuration;

        public profile(IConfiguration configuration)
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
        public ActionResult index()
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
                    try
                    {
                        if (MySQL.checkUserAdmin(connector, token))
                            @TempData["admin"] = "1";
                        else
                            @TempData["admin"] = null;
                    }
                    catch { }
                    string username = MySQL.getUserNameByToken(connector, token);
                    @TempData["Username"] = username;
                    @TempData["viewprofile"] = "viewprofile";
                    dbUser user = MySQL.getUserByToken(connector, token);
                    if (user != null)
                    {
                        @TempData["username"] = user.user;
                        @TempData["id"] = user.id;
                        @TempData["email"] = user.email;
                        @TempData["surname"] = user.surname;
                        @TempData["firstname"] = user.firstname;
                        @TempData["middlename"] = user.middlename;
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
        public ActionResult index(User model)
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
        [HttpGet]
        public IActionResult Edit(editModel model)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            string token = "";
            try
            {
                token = Request.Cookies["user_token"];
                if (token != null)
                {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        try
                        {
                            if (MySQL.checkUserAdmin(connector, token))
                                @TempData["admin"] = "1";
                            else
                                @TempData["admin"] = null;
                        }
                        catch { }
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                    }
                }
                else
                {
                    return RedirectToAction("index", "home");
                }
            }
            catch { }
            dbUser a = MySQL.getUserByToken(connector, token);
            model = new editModel
            {
                id = a.id,
                user = a.user,
                surname = a.surname,
                email = a.email,
                firstname = a.firstname,
                middlename = a.middlename,
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult edit(editModel model)
        {
            Connector connector = getConnector();
            try
            {
                string token = Request.Cookies["user_token"];
                if (token != null)
                {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        try
                        {
                            if (MySQL.checkUserAdmin(connector, token))
                                @TempData["admin"] = "1";
                            else
                                @TempData["admin"] = null;
                        }
                        catch { }
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                    }
                }
            }
            catch { }
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            if (model.user.Length > 50)
            {
                ModelState.AddModelError("user", $"Max 50 знаків");
                return View(model);
            }
            if (!Regex.IsMatch(model.user, @"^[a-zA-Z0-9_]+$"))
            {
                ModelState.AddModelError("user", $"Допустимі лише a-z,A-Z,0-9 та _");
                return View(model);
            }
            if (!Regex.IsMatch(model.email, @"^[a-z0-9_.+-]+@(knu\.ua|gmail\.com)+$"))
            {
                ModelState.AddModelError("email", $"Дупустимі символи a-z,0-9,. та _ доменів knu.ua та gmail.com");
                return View(model);
            }
            if (!Regex.IsMatch(model.firstname, @"^[А-ЯІЇЄ]{1}[а-яіїє']+$"))
            {
                ModelState.AddModelError("firstname", $"Допустимі лише А-Я,а-я");
                return View(model);
            }
            if (!Regex.IsMatch(model.middlename, @"^[А-ЯІЇЄ]{1}[а-яіїє']+$"))
            {
                ModelState.AddModelError("middlename", $"Допустимі лише А-Я,а-я");
                return View(model);
            }
            if (!Regex.IsMatch(model.surname, @"^[А-ЯІЇЄ]{1}[а-яіїє']+$"))
            {
                ModelState.AddModelError("surname", $"Допустимі лише А-Я,а-я");
                return View(model);
            }
            dbUser newUser = new dbUser
            {
                user = model.user,
                email = model.email,
                surname = model.surname,
                firstname = model.firstname,
                middlename = model.middlename
            };
            if (MySQL.adminEditUserById(connector, newUser, model.id))
                return RedirectToAction("index", "profile");
            else
            {
                ModelState.AddModelError("RestoreEmail", $"IE01");
                return View();
            }
        }
        [HttpGet]
        public IActionResult password()
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
                        try
                        {
                            if (MySQL.checkUserAdmin(connector, token))
                                @TempData["admin"] = "1";
                            else
                                @TempData["admin"] = null;
                        }
                        catch { }
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                    }
                }
            }
            catch { }
            return View();
        }
        [HttpPost]
        public IActionResult password(newPasswordModel model)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            string token = "";
            try
            {
                token = Request.Cookies["user_token"];
                if (token != null)
                {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        try
                        {
                            if (MySQL.checkUserAdmin(connector, token))
                                @TempData["admin"] = "1";
                            else
                                @TempData["admin"] = null;
                        }
                        catch { }
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                    }
                }
            }
            catch { }
            dbUser a = MySQL.getUserByToken(connector, token);
            if (MySQL.checkAuth(connector,a.user, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password + a.user))).Replace("-", "").ToLower()) ==a.id)
            {
                if (model.newPassword != model.newPasswordCheck)
                {
                    @TempData["Error"]=$"Паролі не співпадають!";
                    return View(model);
                }
                else
                {
                    MySQL.userUpdatePassword(connector, a.id, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.newPasswordCheck + a.user))).Replace("-", "").ToLower());
                    return RedirectToAction("index");
                }
            }
            else
            {
                @TempData["Error"]=$"Пароль невірний";
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult delete(int id)
        {
            if (Request.Cookies["user_token"] == null)
            {
                return RedirectToAction("index", "home");
            }
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
                            try
                            {
                                if (MySQL.checkUserAdmin(connector, token))
                                    @TempData["admin"] = "1";
                                else
                                    @TempData["admin"] = null;
                            }
                            catch { }
                            @TempData["Username"] = username;
                            @TempData["viewprofile"] = "viewprofile";
                        }

                }
            }
            catch { }
            return View();
        }
        [HttpPost]
        public IActionResult delete(dbUser user)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            string token = "";
            try
            {
                token = Request.Cookies["user_token"];
                if (token != null)
                {
                    string username = MySQL.getUserNameByToken(connector, token);
                    if (username != "IE01")
                    {
                        try
                        {
                            if (MySQL.checkUserAdmin(connector, token))
                                @TempData["admin"] = "1";
                            else
                                @TempData["admin"] = null;
                        }
                        catch { }
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                    }
                }
            }
            catch { }
            if (token == "")
            {
                return RedirectToAction("index", "home");
            }
            dbUser a = MySQL.getUserByToken(connector, token);
            if (user.id != 0)
            {
                if (MySQL.userDeleteProfile(connector, user.id))
                {
                    Response.Cookies.Delete("user_token");
                    return RedirectToAction("index", "home");
                }
            }
            else
            {
                if (MySQL.userDeleteProfile(connector, a.id))
                {
                    Response.Cookies.Delete("user_token");
                    @TempData["Username"] = null;
                    @TempData["viewprofile"] = null;
                    return RedirectToAction("index", "home");
                }
            }
            return View();
        }
    }
}
