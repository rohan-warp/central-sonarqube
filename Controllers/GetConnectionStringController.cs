using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Models;

namespace Storefront.Central.Web.Merchant.Controllers
{
	public class GetConnectionStringController : BaseController
	{
		#region Private Properties

		private IDataRepository<License> SiteRepo { get; }
		private IDataRepository<Domain> DomainRepo { get; }

		#endregion

		#region Constructor

		public GetConnectionStringController(
				IDataRepository<License> siteRepo,
				IDataRepository<Domain> domainRepo
			)
		{
			SiteRepo = siteRepo;
			DomainRepo = domainRepo;
		}

		#endregion

		#region Actions

		[HttpGet, Route("site/{storeRef}/connection-string")]
		public async Task<String> Get(String storeRef)
		{
			var site = await SiteRepo.Table.FirstOrDefaultAsync(a => a.StoreRef == storeRef);

			if (site == null)
				return "";

			return site.DbServer;
		}

		[HttpGet, Route("site/connection-string")]
		public async Task<String> GetByHost(String host)
		{
			var site = await (from a in DomainRepo.Table
							  where a.Hostname.Equals(host, StringComparison.InvariantCultureIgnoreCase)
							  join s in SiteRepo.Table on a.LicenseId equals s.LicenseId
							  select s).FirstOrDefaultAsync();

			if (site == null)
				return "";

			return site.DbServer;
		}

		#endregion
	}
}
