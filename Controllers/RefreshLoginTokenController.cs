using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Storefront.Central.Services.Authentication;

namespace Storefront.Central.Web.Merchant.Controllers
{
	[EnableCors("CentralPortalCorsPolicy")]
	public class RefreshLoginTokenController : Controller
	{
		#region Private Properties

		private IAuthService Auth { get; }

		#endregion

		#region Constructor

		public RefreshLoginTokenController(
				IAuthService auth
			)
		{
			Auth = auth;
		}

		#endregion

		#region Actions

		[HttpGet, Route("refresh/{refresh-token}")]
		public async Task<AuthModel> Get(String refreshToken)
		{
			return await Auth.RefreshLogin(refreshToken);
		}

		#endregion
	}
}
