using System;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Kakadu.Common.Extensions;

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
        private readonly IDistributedCache _cache;

        public RecordController(ILogger<RecordController> logger, IAnonymousServiceHttpClient serviceClient, IDistributedCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet("start/{serviceCode}")]
        public async Task<ActionResult> StartRecordingAsync(string serviceCode, CancellationToken cancellationToken)
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
                await _cache.SetStringAsync(KakaduConstants.ACCESS_TOKEN, authToken.ToString(), new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });

                await _cache.SetAsync<bool>(recordCacheKey, true, new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });

                return Ok(true);
            }

            return Unauthorized();
        }

        [HttpGet("stop/{serviceCode}")]
        public async Task<ActionResult> StopRecordingAsync(string serviceCode, CancellationToken cancellationToken)
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
        [HttpGet("status/{serviceCode}")]
        public async Task<ActionResult<bool>> GetStatusAsync(string serviceCode, CancellationToken cancellationToken)
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
                bool? isRecording = await _cache.GetAsync<bool?>(recordCacheKey, cancellationToken);
                return Ok(isRecording.HasValue && isRecording.Value);
            }
            
            return Unauthorized();
        }
    }
}