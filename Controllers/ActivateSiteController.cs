using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Models;
using Storefront.Central.Web.Merchant.Models;

namespace Storefront.Central.Web.Merchant.Controllers
{
	public class ActivateSiteController : BaseController
	{
		#region Private Properties

		private IDataRepository<License> SiteRepo { get; }

		#endregion

		#region Constructor

		public ActivateSiteController(IDataRepository<License> siteRepo)
		{
			SiteRepo = siteRepo;
		}

		#endregion

		#region Actions

		[HttpPost, Route("site/{storeRef}/activate")]
		public async Task<IActionResult> Activate(String storeRef, [FromBody]ActivateModel model)
		{
			var site = await SiteRepo.Table.FirstOrDefaultAsync(a => a.StoreRef == storeRef);

			if (site == null)
				return NotFound();

			site.DbServer = model.DbServer;
			site.DbSchema = model.DbSchema;
			site.SystemUser = model.SystemUser;
			site.SystemPassword = model.SystemPassword;
			site.ApplicationKey = model.ApplicationKey;

			await SiteRepo.SaveChanges();

			return Ok();
		}
		
		#endregion
	}
}
