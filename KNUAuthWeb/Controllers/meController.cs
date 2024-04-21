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
    public class me : Controller
    {

        // GET: /xxx/Register
        public ActionResult viewprofile()
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
            try
            {
                string token = Request.Cookies["user_token"];
                if (token != null)
                {
                    dbUser user = MySQL.getUserByToken(connector, token);
                    if (user != null)
                    {
                        @TempData["name"] = user.user;
                        @TempData["id"] = user.id;
                        @TempData["email"] = user.email;
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
        public ActionResult viewprofile(User model)
        {
            
            //if (ModelState.IsValid)
            //{
            //    Connector connector = new Connector();
            //    connector.database = "test";
            //    connector.port = 3306;
            //    connector.user = "root";
            //    connector.password = "Qw123456";
            //    connector.server = "localhost";
            //    // Проверка, не занят ли уже такой Username
            //    bool existingUser = MySQL.firstOfDefault(connector,model.Username); 
            //    if (!existingUser)
            //    {
            //        ModelState.AddModelError("Username", $"This username is already taken. {model.Username}");
            //        return View(model);
            //    }

            //    // Добавление нового пользователя в базу данных
            //    MySQL.addUser(connector,model.Username, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password+model.Username))).Replace("-", "").ToLower());

            //    // После успешной регистрации можно перенаправить пользователя на другую страницу
            //    return RedirectToAction("login");
            //}
            return View(model);
        }
    }
}
