﻿using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;
using KNUOAuthApi;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
                else
                {
                    return RedirectToAction("Index", "Home");
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
        [HttpGet]
        public IActionResult edit(editModel model, int id)
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            dbUser a = MySQL.adminGetUserById(connector, id);
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
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch { }
            model = new editModel { 
                id=a.id,
                user=a.user,
                surname=a.surname,
                email=a.email,
                firstname=a.firstname,
                middlename=a.middlename,
            };
            ModelState.AddModelError("id", $"");
            ModelState.AddModelError("user", $"");
            ModelState.AddModelError("surname", $"");
            ModelState.AddModelError("email", $"");
            ModelState.AddModelError("firstname", $"");
            ModelState.AddModelError("middlename", $"");
            return View(model);
        }
        [HttpPost]
        public IActionResult edit(editModel model)
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
                            return RedirectToAction("Index", "Home");
                        }

                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch { }
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
            (0,
                model.user,
                model.email,
                model.surname,
                model.firstname,
                model.middlename
            );
            if (MySQL.adminEditUserById(connector, newUser, model.id))
                return RedirectToAction("index","admin", new { id = model.id });
            else {
                ModelState.AddModelError("RestoreEmail", $"IE01");
                return View();
            }
        }
        [HttpPost]
        public IActionResult delete(int id)
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
                            {
                                @TempData["admin"] = null;
                                return RedirectToAction("index", "home");
                            }
                                
                        }
                        catch { }
                        @TempData["Username"] = username;
                        @TempData["viewprofile"] = "viewprofile";
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch { }
            if (token == "")
            {
                return RedirectToAction("index", "home");
            }
            dbUser a = MySQL.getUserByToken(connector, token);
            if (id != 0)
            {
                if (MySQL.userDeleteProfile(connector, id))
                {
                    return RedirectToAction("index", "admin");
                }
            }
            else
            {
                if (MySQL.userDeleteProfile(connector, a.id))
                {
                    return RedirectToAction("index", "admin");
                }
            }
            return View();
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
