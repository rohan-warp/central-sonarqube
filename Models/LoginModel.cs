using System;

namespace Storefront.Central.Web.Merchant.Models
{
	public class LoginModel
	{
		public String Username { get; set; }
		public String Password { get; set; }
		public String CallbackUrl { get; set; }
		public String RedirectUrl { get; set; }
	}
}
