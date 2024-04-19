using System;
namespace KNUOAuthApi
{
	public class Token
	{
		public string acces_token {  get; set; }
		public int expires_in { get; set;}
		public string refresh_token { get; set; }
	}
}
