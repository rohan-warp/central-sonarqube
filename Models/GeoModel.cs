using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Storefront.Central.Web.Merchant.Models
{
	public class GeoModel
	{
		public String Country { get; set; }
		public String Currency { get; set; }
		public Int32 TimeZone { get; set; }
		public String Language { get; set; }
	}
}
