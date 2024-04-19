using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using KNUAuthMYSQLConnector;


namespace KNUOAuthApi.Controllers
{
    [Route("/api/v2.5/[controller]")]
    [ApiController]
    public class token : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(
            )
        {
            Connector connector = new Connector();
            connector.database = "test";
            connector.port = 3306;
            connector.user = "root";
            connector.password = "Qw123456";
            connector.server = "localhost";
            string type = "", cSecret = "", authCode = "", reURI = "", refresh_token = ""; int cID = 0;
            if (HttpContext.Request.Query.ContainsKey("grant_type")) { type = HttpContext.Request.Query["grant_type"]; if (type == null) { return StatusCode(500, "Error: grant_type is Empty!"); } }
            if (HttpContext.Request.Query.ContainsKey("client_id")) {  cID = int.Parse(HttpContext.Request.Query["client_id"]); }
            if (HttpContext.Request.Query.ContainsKey("client_secret")) {  cSecret = HttpContext.Request.Query["client_secret"]; }
            if (HttpContext.Request.Query.ContainsKey("code")) {  authCode = HttpContext.Request.Query["code"]; }
            if (HttpContext.Request.Query.ContainsKey("redirect_uri")) {  reURI = HttpContext.Request.Query["redirect_uri"]; }
            if (HttpContext.Request.Query.ContainsKey("refresh_token")) {  refresh_token = HttpContext.Request.Query["refresh_token"]; }
            var token = new Token { };                                                        // 1 3
            if (type.ToLower() == "bearer")
            {

                if (cID == 0) { return StatusCode(500, "Error: client_id is Empty!"); }
                if (cSecret == null) { return StatusCode(500, "Error: client_secret is Empty!"); }
                if (authCode == null) { return StatusCode(500, "Error: code is Empty!"); }
                if (reURI == null) { return StatusCode(500, "Error: redirect_uri is Empty!"); }
                token = new Token
                {
                    acces_token = genToken(cID + authCode + DateTime.Now.Ticks),
                    expires_in = 86400,
                    refresh_token = genToken(cID + authCode + cSecret + DateTime.Now.Ticks)
                };
                MySQL.AddTokenB(connector, token.acces_token, cID, token.refresh_token, token.expires_in, "bearer");
            }
            else if (type.ToLower() == "refresh_token")
            {
                if (refresh_token == null) { return StatusCode(500, "Error: refresh_token is Empty!"); }
                token = new Token
                {
                    acces_token = genToken(refresh_token + DateTime.Now.Ticks),
                    expires_in = 86400,
                    refresh_token = genToken(refresh_token + DateTime.Now.Ticks + 1)
                };
                bool check = MySQL.refreshTokenR(connector,refresh_token,token.acces_token,token.refresh_token,token.expires_in);
                if (check) {
                    return Ok(JsonSerializer.Serialize(token));
                }else{
                    return StatusCode(500, "Internal server error!");
                }
            }
            else {
                return StatusCode(500, "Internal server error!");
            }
            return Ok(JsonSerializer.Serialize(token));
        }
        //public ContentResult Index()
        //{
        //    return new ContentResult
        //    {
        //        ContentType = "text/html",
        //        StatusCode = (int)HttpStatusCode.OK,Content = "<html><body>Hello World</body></html>"
        //    };
        //}
        public string genToken(string Seed)
        {
            return BitConverter.ToString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(Seed))).Replace("-", "").ToLower();
        }
    }
    
    }