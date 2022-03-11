using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Extensions;
using Storefront.Central.Core.Models;
using Storefront.Central.Web.Managers;
using Storefront.ServiceBus.Models.Signup;
using Storefront.Central.Core.Logging;
using CoreMerchant = Storefront.Central.Core.Models.Merchant;
using Storefront.Central.Services.GraphQL.Models;
using Storefront.Central.Core.Models.Enums;

namespace Storefront.Central.Web.Merchant.Controllers
{
	[Route("setup")]
	[EnableCors("CentralPortalCorsPolicy")]
	public class SetupController : Controller
	{
		#region Private Properties

		private IDataRepository<Channel> ChannelRepo { get; }
		private IDataRepository<CoreMerchant> MerchantRepo { get; }
		private IDataRepository<License> SiteRepo { get; }
		private IDataRepository<SiteData> SiteDataRepo { get; }

		private ILogger Logger { get; }

		private IGeolocationManager GeolocationManager { get; }

		#endregion

		#region Constructor

		public SetupController(
				IDataRepository<Channel> channelRepo,
				IDataRepository<CoreMerchant> merchantRepo,
				IDataRepository<License> siteRepo,
				IDataRepository<SiteData> siteDataRepo,
				ILogger logger,
				IGeolocationManager geolocationManager
			)
		{
			ChannelRepo = channelRepo;
			MerchantRepo = merchantRepo;
			SiteRepo = siteRepo;
			SiteDataRepo = siteDataRepo;

			Logger = logger;

			GeolocationManager = geolocationManager;
		}

		#endregion

		#region Actions

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]Dictionary<String, Object> form)
		{
			try
			{
				// Get Model
				var model = GetSetupModel(form);

				// Get Site
				var site = await GetSite(model.SiteCode);

				// Get Channel
				var channel = await GetChannel(site.ChannelId);

				// Validate Model
				if (!ModelValid(form, channel.SecretKey, model.SecureHash))
					return BadRequest();

				var geolocation = GeolocationManager.Get();
				if (geolocation != null)
				{
					model.Data.Add("Geo.Currency", geolocation.Currency);
					model.Data.Add("Geo.Country", geolocation.Country);
					model.Data.Add("Geo.TimeZone", geolocation.TimeZone.ToString());
					model.Data.Add("Geo.Language", geolocation.Language);
				}

				// Get Merchant
				var merchant = await GetMerchant(site.MerchantId);

				// Update Merchant
				await Update(merchant, model);
				                                                                           
				// Add Site Data
				await AddSiteData(site.LicenseId, model.Data);

				using (var serviceBus = ServiceBus.ServiceBus.CreateInstance())
				{
					await serviceBus.SetupComplete(new SetupCompleteModel()
					{
						Code = site.Code
					});
				}

				// Redirect to application URL // TODO: Discuss with JJ what return type to use here
				return Ok();
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}

			return BadRequest();
		}

		#endregion

		#region Private Methods

		private SetupModel GetSetupModel(Dictionary<String, Object> form)
		{
			var setupModel = new SetupModel()
			{
				Data = new Dictionary<String, String>()
			};

			foreach (var key in form.Keys)
			{
				var value = form[key] as String;

				switch (key)
				{
					case "SiteCode":
						setupModel.SiteCode = value;
						break;

					case "Name":
						setupModel.Name = value;
						break;

					case "Surname":
						setupModel.Surname = value;
						break;

					case "Phone":
						setupModel.Phone = value;
						break;

					case "SecureHash":
						setupModel.SecureHash = value;
						break;

					default:
						{
							if (setupModel.Data.ContainsKey(key))
								continue;

							setupModel.Data.Add(key, value);

							break;
						}
				}
			}

			return setupModel;
		}

		private async Task<License> GetSite(String code)
		{
			return await SiteRepo.Table.FirstOrDefaultAsync(a => a.Code == code);
		}

		private async Task<Channel> GetChannel(Int32 channelId)
		{
			return await ChannelRepo.Table.FirstOrDefaultAsync(a => a.ChannelId == channelId);
		}

		private Boolean ModelValid(Dictionary<String, Object> form, String secret, String secureHash)
		{
			var str = String.Join("", form.Where(a => a.Key != "SecureHash").OrderBy(a => a.Key).Select(a => $"{a.Key}:{a.Value}"));

			var result = str.ToLowerInvariant().HashString(secret);

			return result.Equals(secureHash);
		}

		private async Task<MerchantModel> GetMerchant(Int32 merchantId)
		{
			var entity = await MerchantRepo.Table.FirstOrDefaultAsync(a => a.MerchantId == merchantId);
			return new MerchantModel
			{
				Id = entity.MerchantId,
				AlternateEmail = entity.AlternateEmail,
				AlternatePhone = entity.AlternatePhone,
				Code = entity.Code,
				Email = entity.Email,
				Name = entity.Name,
				Surname = entity.Surname,
				IsVerified = entity.IsVerified,
				LoginId = entity.LoginId,
				Phone = entity.Phone,
				TwoFactorAuth = entity.Login.TwoFactorAuth,
				CreditCards = entity.CreditCards.Select(c => new Services.GraphQL.Models.CreditCards.CreditCardModel
				{
					Id = c.CreditCardId,
					Mask = c.Mask,
					Provider = c.PaymentProvider.Name
				}).ToHashSet(),
				Licenses = entity.Licenses.Select(l => new LicenseModel
				{
					Id = l.LicenseId,
					Active = l.Status == LicenseStatus.Active,
					ChannelId = l.ChannelId,
					Code = l.Code,
					CountryCode = l.CountryCode,
					HostName = l.HostName,
					Name = l.Name,
					StatusEnum = l.StatusEnum,
					Status = l.Status
				}).ToHashSet()
			};
		}

		private async Task Update(MerchantModel merchant, SetupModel model)
		{
			merchant.Name = model.Name;
			merchant.Surname = model.Surname;
			merchant.Phone = model.Phone;

			var entity = await MerchantRepo.Table.FirstOrDefaultAsync(a => a.MerchantId == merchant.Id);
			entity.Name = model.Name;
			entity.Surname = model.Surname;
			entity.Phone = model.Phone;

			await MerchantRepo.SaveChanges();
		}

		private async Task AddSiteData(Int32 siteId, Dictionary<String, String> data)
		{
			await SiteDataRepo.Add((from d in data
									select new SiteData()
									{
										CreatedOnUtc = DateTime.UtcNow,
										LicenseId = siteId,
										Key = d.Key,
										Value = d.Value
									}).ToList());
		}

		#endregion

		#region Private Classes

		private class SetupModel
		{
			public String SiteCode { get; set; }
			public String Name { get; set; }
			public String Surname { get; set; }
			public String Phone { get; set; }

			public Dictionary<String, String> Data { get; set; }

			public String SecureHash { get; set; }
		}

		#endregion
	}
}