using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Central.Web.Merchant.Models;
using Storefront.Central.Core.Data;
using Storefront.Central.Core.Logging;
using Storefront.Central.Core.Models;
using Storefront.Central.Services.Authentication;
using CoreMerchant = Storefront.Central.Core.Models.Merchant;

namespace Storefront.Central.Web.Merchant.Controllers
{
    [EnableCors("CentralPortalCorsPolicy")]
    [Route("login/merchant")]
    public class LoginController : Controller
	{
		#region Private Properties

		private ILogger Logger { get; }

		private IDataRepository<Login> LoginRepo { get; }
		private IDataRepository<CoreMerchant> MerchantRepo { get; }

		private IAuthService Auth { get; }

		#endregion

		#region Constructor

		public LoginController(
				ILogger logger,
				IDataRepository<Login> loginRepo,
				IDataRepository<CoreMerchant> merchantRepo,
				IAuthService auth
			)
		{
			Logger = logger;

			LoginRepo = loginRepo;
			MerchantRepo = merchantRepo;

			Auth = auth;
		}

        #endregion

        #region Actions
        
        [HttpPost]
        public async Task<LoginResultModel> Post([FromBody] LoginModel model)
        {
            try
            {
                var auth = await Auth.Login(model.Username, model.Password);

                if (auth == null)
                    return new LoginResultModel
                    {
                        Success = false,
                        Message = "Username or password incorrect"
                    };

                if (!auth.Success)
                    return new LoginResultModel()
                    {
                        Success = false,
                        Message = auth.Message
                    };

                var login = await LoginRepo.Table.FirstOrDefaultAsync(a => a.Username == model.Username);

                if (login == null)
                    return new LoginResultModel
                    {
                        Success = false,
                        Message = "Account not recognized"
                    };

                var merchant = await MerchantRepo.Table.FirstOrDefaultAsync(a => a.LoginId == login.LoginId);

                if (merchant == null)
                    return new LoginResultModel
                    {
                        Success = false,
                        Message = "Account not recognized"
                    };

                var url = model.RedirectUrl;
                if (String.IsNullOrWhiteSpace(url))
                    url = "/merchant/profile";

                // Redirect to callback URL
                return new LoginResultModel
                {
                    Success = true,
                    Action = LoginAction.Redirect,
                    Location = url,
                    IdToken = auth.IdToken,
                    AccessToken = auth.AccessToken,
                    RefreshToken = auth.RefreshToken
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return new LoginResultModel
            {
                Success = false,
                Message = "Bad request"
            };
        }

        #endregion
    }
}