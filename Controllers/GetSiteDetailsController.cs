using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Models;
using Storefront.Central.Web.Merchant.Models;
using CoreMerchant = Storefront.Central.Core.Models.Merchant;

namespace Storefront.Central.Web.Merchant.Controllers
{
	public class GetSiteDetailsController : BaseController
	{
		#region Private Properties

		private IDataRepository<License> SiteRepo { get; }
		private IDataRepository<SiteData> SiteDataRepo { get; }
		private IDataRepository<CoreMerchant> MerchantRepo { get; }
		private IDataRepository<Login> LoginRepo { get; }
		private IDataRepository<Channel> ChannelRepo { get; }

		#endregion

		#region Constructor

		public GetSiteDetailsController(
				IDataRepository<License> siteRepo, 
				IDataRepository<SiteData> siteDataRepo, 
				IDataRepository<CoreMerchant> merchantRepo, 
				IDataRepository<Login> loginRepo, 
				IDataRepository<Channel> channelRepo
			)
		{
			SiteRepo = siteRepo;
			SiteDataRepo = siteDataRepo;
			MerchantRepo = merchantRepo;
			LoginRepo = loginRepo;
			ChannelRepo = channelRepo;
		}

		#endregion

		#region Actions

		[HttpGet, Route("site/{siteCode}/details")]
		public async Task<SiteModel> Get(String siteCode)
		{
			var site = await SiteRepo.Table.FirstOrDefaultAsync(a => a.Code == siteCode);

			if (site == null)
				return null;

			var merchant = await MerchantRepo.Table.FirstOrDefaultAsync(a => a.MerchantId == site.MerchantId);
			var login = await LoginRepo.Table.FirstOrDefaultAsync(a => a.LoginId == merchant.LoginId);
			var channel = await ChannelRepo.Table.FirstOrDefaultAsync(a => a.ChannelId == site.ChannelId);
			var siteData = await SiteDataRepo.Table.Where(a => a.LicenseId == site.LicenseId).ToDictionaryAsync(a=> a.Key, b => b.Value);

			var timeZone = siteData.ContainsKey("Geo.TimeZone") ? siteData["Geo.TimeZone"] : "";
			Int32.TryParse(timeZone, out var timeZoneInt);

			return new SiteModel()
			{
				Name = site.Name,
				Email = merchant.Email,
				Phone = merchant.Phone,
				Merchant = new MerchantModel()
				{
					Email = merchant.Email,
					Phone = merchant.Phone,
					Surname = merchant.Surname,
					FirstName = merchant.Name
				},
				Api = new ApiDetailsModel()
				{
					Url = site.HostName,
					Key = site.ApplicationKey,
					Username = site.SystemUser,
					Password = site.SystemPassword
				},
				Channel = new ChannelModel()
				{
					Domain = site.HostName
				},
				Geo = new GeoModel()
				{
					Country = siteData.ContainsKey("Geo.Country") ? siteData["Geo.Country"] : "",
					Currency = siteData.ContainsKey("Geo.Currency") ? siteData["Geo.Currency"] : "",
					Language = siteData.ContainsKey("Geo.Language") ? siteData["Geo.Language"] : "",
					TimeZone = timeZoneInt
				}
			};
		}

		#endregion
	}
}
