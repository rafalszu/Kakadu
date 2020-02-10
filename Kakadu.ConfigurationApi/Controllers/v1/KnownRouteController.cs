using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Authorize]
    public class KnownRouteController : ControllerBase
    {
        private readonly IAppCache _cache;
        private readonly ILogger<KnownRouteController> _logger;

        public KnownRouteController(ILogger<KnownRouteController> logger, IAppCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("store/{serviceCode}")]
        public async Task<ActionResult> StoreFoundRouteInCache(string serviceCode, [FromBody]KnownRouteDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException();

            string cachekey = KakaduConstants.GetFoundRoutesKey(serviceCode);
            List<KnownRouteDTO> list = await _cache.GetAsync<List<KnownRouteDTO>>(cachekey);
            if(list == null)
                list = new List<KnownRouteDTO>();

            bool contains = list.Any(route => route.RelativeUrl.Equals(dto.RelativeUrl, StringComparison.InvariantCultureIgnoreCase) && route.MethodName.Equals(dto.MethodName, StringComparison.InvariantCultureIgnoreCase));
            if(!contains)
            {
                list.Add(dto);
                _cache.Add(cachekey, list, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions {
                    Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.NeverRemove
                });
            }

            return Ok();
        }
    }
}