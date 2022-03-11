using System;

namespace Storefront.Central.Web.Merchant.Models
{
	public class LoginResultModel
	{
		public Boolean Success { get; set; }
		public String Message { get; set; }
		public LoginAction Action { get; set; }
		public String Location { get; set; }
		public String IdToken { get; set; }
		public String AccessToken { get; set; }
		public String RefreshToken { get; set; }
	}
}
