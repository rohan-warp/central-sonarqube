using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Core.Crypto;
using Storefront.Central.Core.Data;
using Storefront.Central.Web.Merchant.Models;
using CoreMerchant = Storefront.Central.Core.Models.Merchant;


namespace Storefront.Central.Web.Merchant.Controllers
{
	[EnableCors("CentralPortalCorsPolicy")]
	[Route("accounts/merchants/sign-up")]
	public class SignUpController : BaseController
	{
		#region Private Properties

		private IDataRepository<CoreMerchant> MerchantRepo { get; }

		#endregion

		#region Constructor

		public SignUpController(IDataRepository<CoreMerchant> merchantRepo)
		{
			MerchantRepo = merchantRepo;
		}

		#endregion

		#region Actions

		[HttpPost]
		public async Task<IActionResult> SignUp([FromBody]SignUpModel model)
		{
			try
			{
				// Check model valid
				if (!IsValid(model))
					return Problem(statusCode: 400, detail: "Model is invalid");

				// Check email doesn't already exist
				if (await EmailExists(model.Email))
					return Problem(statusCode: 400, detail: "Email already registered");

				// Generate Code
				var code = await GenerateCode();

				//// HashPassword Password
				//var (password, salt) = Password.HashPassword(model.Password);

				// Create Merchant
				var merchant = new CoreMerchant()
				{
					CreatedOnUtc = DateTime.UtcNow,
					ModifiedOnUtc = DateTime.UtcNow,
					Name = model.Name,
					Surname = model.Surname,
					Code = code,
					Email = model.Email,
					Phone = model.Phone,
					AlternateEmail = model.AlternateEmail,
				};
				await MerchantRepo.Add(merchant);

				await MerchantRepo.SaveChanges();

				return Created($"/accounts/merchants/{merchant.Code}", merchant.MerchantId);
			}
			catch (Exception)
			{
				// Logger
			}

			return BadRequest();
		}

		#endregion

		#region Private Methods

		private Boolean IsValid(SignUpModel model)
		{
			return false;
		}

		private async Task<Boolean> EmailExists(String email)
		{
			return await MerchantRepo.Table.AnyAsync(a => a.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
		}

		private async Task<String> GenerateCode()
		{
			async Task<Boolean> CodeExists(String code) => await MerchantRepo.Table.AnyAsync(a => a.Code == code);

			do
			{
				var code = BaseX.GenerateString(32);

				if (!(await CodeExists(code)))
					return code;
			} while (true);
		}

		#endregion
	}
}
