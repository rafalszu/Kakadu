using System;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Kakadu.ActionApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class RecordController : ControllerBase
    {
        private readonly ILogger<RecordController> _logger;
        private readonly IAnonymousServiceHttpClient _serviceClient;
        private readonly IAppCache _cache;

        public RecordController(ILogger<RecordController> logger, IAnonymousServiceHttpClient serviceClient, IAppCache cache)
        {
            _logger = logger;
            _serviceClient = serviceClient;
            _cache = cache;
        }

        [HttpGet("start/{serviceCode}")]
        public async Task<ActionResult> StartRecording(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            if(!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new HttpBadRequestException("missing authorization header");

            string recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);

            // verify bearer token from headers with configuration api
            var isValid = await _serviceClient.ValidateTokenAsync(authToken, cancellationToken);
            if(isValid)
            {
                _cache.Add(KakaduConstants.ACCESS_TOKEN, authToken, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) 
                });

                _cache.Add(recordCacheKey, true, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions {
                    Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.NeverRemove
                });

                return Ok(true);
            }

            return Unauthorized();
        }

        [HttpGet("stop/{serviceCode}")]
        public async Task<ActionResult> StopRecording(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            if(!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new HttpBadRequestException("missing authorization header");

            string recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);

            // verify bearer token from headers with configuration api
            var isValid = await _serviceClient.ValidateTokenAsync(authToken, cancellationToken);
            if(isValid)
            {
                _cache.Remove(recordCacheKey);
                return Ok(true);
            }
            
            return Unauthorized();
        }
    }
}