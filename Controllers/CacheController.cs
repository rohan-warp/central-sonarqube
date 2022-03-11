using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Storefront.Central.Web.Managers;

namespace Storefront.Central.Web.Merchant.Controllers
{
	[Route("cache")]
    public class CacheController : Controller
    {
		#region Private Properties

		private ICacheManager Cache { get; }

		#endregion

		public CacheController(
				ICacheManager cache	
			)
		{
			Cache = cache;
		}

		[Route("list")]
		[HttpGet]
		public List<String> List()
		{
			return Cache.GetKeys();
		}

		[Route("get")]
		[HttpGet]
		public async Task<dynamic> Get(String key)
		{
			return await Cache.Get<dynamic>(key);
		}

		[Route("clear")]
		[HttpDelete]
        public IActionResult Clear()
        {
			Cache.Clear();

            return Ok();
        }
    }
}