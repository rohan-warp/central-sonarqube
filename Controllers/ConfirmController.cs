using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Models.Enums;
using Storefront.Central.Core.Models;

namespace Storefront.Central.Web.Merchant.Controllers
{
	public class ConfirmController : BaseController
	{
		#region Private Properties

		private IDataRepository<License> SiteRepo { get; }
		private IDataRepository<Domain> DomainRepo { get; }

		#endregion

		#region Constructor

		public ConfirmController(IDataRepository<License> siteRepo, IDataRepository<Domain> domainRepo)
		{
			SiteRepo = siteRepo;
			DomainRepo = domainRepo;
		}

		#endregion

		#region Actions

		[HttpGet, Route("site/confirm")]
		public async Task<IActionResult> GetByHost(String host)
		{
			var site = await (from a in DomainRepo.Table
							  where a.Hostname.Equals(host, StringComparison.InvariantCultureIgnoreCase)
							  join s in SiteRepo.Table on a.LicenseId equals s.LicenseId
							  select s).FirstOrDefaultAsync();

			if (site == null)
				return NotFound();

			if (site.Status == LicenseStatus.Pending || site.Status == LicenseStatus.Active)
				return Ok();

			return NotFound();
		}

		#endregion
	}
}
