using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using KNUAuthMYSQLConnector;
using KNUAuthWeb.Models;


namespace KNUOAuthApi.Controllers
{
    [Route("/oauth/[controller]")]
    [ApiController]
    public class tokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public tokenController(IConfiguration configuration)
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
        [HttpPost]
        public IActionResult Post(
            )
        {
            Connector connector = getConnector();
            if (connector.user == null | connector.port == 0 | connector.user == null | connector.password == null | connector.server == null) { return StatusCode(500, "Wrong server configuration!"); }
            string type = "", cSecret = "", authCode = "", reURI = "", refresh_token = "", username = "", password = "", scope = ""; int cID = 0;
            if (HttpContext.Request.Query.ContainsKey("grant_type")) { type = HttpContext.Request.Query["grant_type"]; if (type == null) { return StatusCode(500, "Error: grant_type is Empty!"); } }
            if (HttpContext.Request.Query.ContainsKey("client_id")) {  cID = int.Parse(HttpContext.Request.Query["client_id"]); }
            if (HttpContext.Request.Query.ContainsKey("client_secret")) {  cSecret = HttpContext.Request.Query["client_secret"]; }
            if (HttpContext.Request.Query.ContainsKey("code")) {  authCode = HttpContext.Request.Query["code"]; }
            if (HttpContext.Request.Query.ContainsKey("redirect_uri")) {  reURI = HttpContext.Request.Query["redirect_uri"]; }
            if (HttpContext.Request.Query.ContainsKey("refresh_token")) {  refresh_token = HttpContext.Request.Query["refresh_token"]; }
            if (HttpContext.Request.Query.ContainsKey("username")) { username = HttpContext.Request.Query["username"]; }
            if (HttpContext.Request.Query.ContainsKey("password")) { password = HttpContext.Request.Query["password"]; }
            if (HttpContext.Request.Query.ContainsKey("scope")) { scope = HttpContext.Request.Query["scope"]; }
            var token = new Token { };                                                        // 1 3
            if (type.ToLower() == "authorization_code")
            {

                if (cID == 0) { return StatusCode(500, "Error: client_id is Empty!"); }
                if (cSecret == null) { return StatusCode(500, "Error: client_secret is Empty!"); }
                if (authCode == null) { return StatusCode(500, "Error: code is Empty!"); }
                if (reURI == null) { return StatusCode(500, "Error: redirect_uri is Empty!"); }
                int user_id = MySQL.getUserIdByCode(connector, authCode);
                string codeScope = MySQL.checkCode(connector, authCode, cID);
                if (codeScope!=null & codeScope!="IE01" & codeScope!= "TS_EXPIRED")
                {
                    token = new Token
                    {
                        type = "bearer",
                        scope = codeScope,
                        acces_token = genToken(cID + authCode + user_id + DateTime.Now.Ticks),
                        expires_in = int.Parse(_configuration["tokenExpTime"]),
                        refresh_token = genToken(cID + authCode + cSecret + user_id + DateTime.Now.Ticks)
                    };
                    MySQL.AddTokenB(connector, token.acces_token, user_id, token.refresh_token, token.expires_in,token.type,token.scope);
                }else if (codeScope== "TS_EXPIRED")
                {
                    return StatusCode(500, "Code expired!");
                }
                else { return StatusCode(500, "Code or client_id incorrect!"); }
            }
            else if (type.ToLower() == "refresh_token")
            {
                if (refresh_token == null) { return StatusCode(500, "Error: refresh_token is Empty!"); }
                token = new Token
                {
                    type = "bearer",
                    scope = scope,
                    acces_token = genToken(refresh_token + DateTime.Now.Ticks),
                    expires_in = int.Parse(_configuration["tokenExpTime"]),
                    refresh_token = genToken(refresh_token + DateTime.Now.Ticks + 1)
                };
                bool check = MySQL.refreshTokenR(connector,refresh_token,token.acces_token,token.refresh_token,token.expires_in);
                if (check) {
                    return Ok(JsonSerializer.Serialize(token));
                }else{
                    return StatusCode(500, "Internal server error!");
                }
            }
            else if (type.ToLower() == "password")
            {
                if (username == null) { return StatusCode(500, "Error: username is Empty!"); }
                if (password == null) { return StatusCode(500, "Error: password is Empty!"); }
                if (scope == null) { return StatusCode(500, "Error: scope is Empty!"); }
                int userId = MySQL.checkAuth(connector, username, BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(password + username))).Replace("-", "").ToLower());
                if (userId == 0)
                {
                    return StatusCode(500, "Username or password incorrect!");
                }
                else
                {
                    token = new Token
                    {
                        type = "string",
                        acces_token = genToken(refresh_token + DateTime.Now.Ticks),
                        expires_in = int.Parse(_configuration["tokenExpTime"]),
                        refresh_token = genToken(refresh_token + DateTime.Now.Ticks + 1)
                    };
                }
                bool check = MySQL.AddTokenB(connector, token.acces_token, userId, token.refresh_token, token.expires_in, token.type);
                if (check)
                {
                    return Ok(JsonSerializer.Serialize(token));
                }
                else
                {
                    return StatusCode(500, "Internal server error!");
                }
            }
            else {
                return StatusCode(500, "Internal server error!");
            }
            return Ok(JsonSerializer.Serialize(token));
        }

        public static string genToken(string Seed)
        {
            return BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(Seed))).Replace("-", "").ToLower();
        }
    }
    
    }