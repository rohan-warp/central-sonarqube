using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Storefront.Central.Web.Merchant.Models
{
	public class SiteModel
	{
		public String Name { get; set; }
		public String Email { get; set; }
		public String Phone { get; set; }
		
		public MerchantModel Merchant { get; set; }

		public GeoModel Geo { get; set; }

		public ChannelModel Channel { get; set; }

		public ApiDetailsModel Api { get; set; }
	}
}
