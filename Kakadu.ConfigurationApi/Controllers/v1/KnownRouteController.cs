using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Kakadu.Common.Extensions;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Authorize]
    public class KnownRouteController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<KnownRouteController> _logger;

        public KnownRouteController(ILogger<KnownRouteController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("store/{serviceCode}")]
        public async Task<ActionResult> StoreFoundRouteInCache(string serviceCode, [FromBody]KnownRouteDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException();

            var cachekey = KakaduConstants.GetFoundRoutesKey(serviceCode);
            var list = await _cache.GetAsync<List<KnownRouteDTO>>(cachekey) ?? new List<KnownRouteDTO>();

            var contains = list.Any(route => route.RelativeUrl.Equals(dto.RelativeUrl, StringComparison.InvariantCultureIgnoreCase) && route.MethodName.Equals(dto.MethodName, StringComparison.InvariantCultureIgnoreCase));
            if(!contains)
            {
                list.Add(dto);
                await _cache.SetAsync<List<KnownRouteDTO>>(cachekey, list, new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4)
                });
            }

            return Ok();
        }
    }
}