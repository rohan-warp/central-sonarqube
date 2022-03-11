using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Storefront.Central.Services.Authentication;

namespace Storefront.Central.Web.Merchant.Controllers
{
	[Route("validate")]
	[EnableCors("CentralPortalCorsPolicy")]
	public class ValidateController : Controller
    {
		#region Private Properties

        private IAuthService Auth { get; }

        #endregion

        #region Constructor

		public ValidateController(IAuthService auth)
		{
			Auth = auth;
		}

        #endregion

        #region Actions

        [HttpGet]
		public IActionResult Get(String idToken, String accessToken)
		{
			if (Auth.Verify(idToken, accessToken))
				return Ok();
			return BadRequest();
		}

        #endregion
    }
}