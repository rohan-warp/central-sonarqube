using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Models.Enums;
using Storefront.Central.Services.GraphQL.Models;
using CoreMerchant = Storefront.Central.Core.Models.Merchant;

namespace Storefront.Central.Web.Merchant.Controllers
{
	[Route("accounts/merchants")]
	public class GetMerchantController : BaseController
	{
		#region Private Properties

		private IDataRepository<CoreMerchant> MerchantRepo { get; }

		#endregion

		#region Constructor

		public GetMerchantController(
				IDataRepository<CoreMerchant> merchantRepo
			)
		{
			MerchantRepo = merchantRepo;
		}

		#endregion

		#region Actions

		[HttpGet, Route("{code}")]
		public async Task<IActionResult> Get(String code)
		{
			var merchant = await MerchantRepo.Table.FirstOrDefaultAsync(a => a.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));

			if (merchant == null)
				return NoContent();

			return Ok(new MerchantModel()
			{
				Id = merchant.MerchantId,
				AlternateEmail = merchant.AlternateEmail,
				AlternatePhone = merchant.AlternatePhone,
				Code = merchant.Code,
				Email = merchant.Email,
				Name = merchant.Name,
				Surname = merchant.Surname,
				IsVerified = merchant.IsVerified,
				LoginId = merchant.LoginId,
				Phone = merchant.Phone,
				TwoFactorAuth = merchant.Login.TwoFactorAuth,
				CreditCards = merchant.CreditCards.Select(c => new Services.GraphQL.Models.CreditCards.CreditCardModel
				{
					Id = c.CreditCardId,
					Mask = c.Mask,
					Provider = c.PaymentProvider.Name
				}).ToHashSet(),
				Licenses = merchant.Licenses.Select(l => new LicenseModel
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
			});
		}

		#endregion
	}
}
