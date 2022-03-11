using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Storefront.Central.Web.Merchant.Models
{
	public class ActivateModel
	{
		public String StoreRef { get; set; }
		public String SystemUser { get; set; }
		public String SystemPassword { get; set; }
		public String ApplicationKey { get; set; }
		public String DbServer { get; set; }
		public String DbSchema { get; set; }
	}
}
