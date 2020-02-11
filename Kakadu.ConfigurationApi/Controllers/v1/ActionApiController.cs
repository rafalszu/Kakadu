using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kakadu.DTO.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Kakadu.Common.Extensions;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class ActionApiController : ControllerBase
    {
        private readonly ILogger<ActionApiController> _logger;
        private readonly IDistributedCache _cache;

        public ActionApiController(ILogger<ActionApiController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]string urls)
        {
            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if(instances == null)
                instances = new List<string>();

            bool changed = false;
            foreach(var url in urls.Split(';'))
            {
                if(!instances.Contains(url))
                {
                    instances.Add(url);
                    changed = true;
                }
            }

            if(changed)
            {
                await _cache.SetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES, instances, new DistributedCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365) // hacky way of never removing?
                });
            }

            return Ok();
        }

        [Route("Unregister")]
        [HttpPost]
        public async Task<IActionResult> Unregister([FromBody]string urls)
        {
            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if(instances == null) // dont do anything
                return Ok();

            bool changed = false;
            foreach(var url in urls.Split(';'))
            {
                if(instances.Contains(url))
                {
                    instances.Remove(url);
                    changed = true;
                }
            }

            if(changed)
            {
                await _cache.SetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES, instances, new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
                });
            }

            return Ok();
        }
    }
}