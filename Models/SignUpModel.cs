using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Storefront.Central.Web.Merchant.Models
{
	public class SignUpModel
	{
		public String Name { get; set; }
		public String Surname { get; set; }
		public String Email { get; set; }
		public String Phone { get; set; }
		public String AlternateEmail { get; set; }
		public Boolean TwoFactorAuth { get; set; }
		public String Password { get; set; }
	}
}
