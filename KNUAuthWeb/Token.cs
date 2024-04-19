using System;
using System.Text.Json.Serialization;
namespace KNUOAuthApi
{
	public class Token
	{
        public string type { get; set; }
        public int expires_in { get; set; }
        public string acces_token {  get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string refresh_token { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string scope { get; set; }

    }
}
