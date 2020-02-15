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
using System.Collections.Generic;
using Kakadu.DTO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kakadu.ActionApi.Tests")]
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

            if(await TryValidateTokenAsync(cancellationToken))
            {
                var authToken = HttpContext.Request.Headers["Authorization"];
                string recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);

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

            if(await TryValidateTokenAsync(cancellationToken))
            {
                string recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);
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

            if(await TryValidateTokenAsync(cancellationToken))
            {
                var status = await GetRecordingStatusAsync(serviceCode, cancellationToken);
                return Ok(status?.IsRecording ?? false);
            }

            return Unauthorized();
        }

        [HttpGet("status")]
        public async Task<ActionResult<ServiceCaptureStatusDTO[]>> GetStatusesAsync(List<string> serviceCodes, CancellationToken cancellationToken)
        {
            if(serviceCodes == null || !serviceCodes.Any())
                return null;

            if(await TryValidateTokenAsync(cancellationToken))
            {
                IEnumerable<Task<ServiceCaptureStatusDTO>> tasks = serviceCodes.Select(code => GetRecordingStatusAsync(code, cancellationToken));
                return await Task.WhenAll(tasks);
            }

            return Unauthorized();
        }

        internal async Task<ServiceCaptureStatusDTO> GetRecordingStatusAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(serviceCode);

            string recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);
            
            return new ServiceCaptureStatusDTO {
                ServiceCode = serviceCode,
                IsRecording = (await _cache.GetAsync<bool?>(recordCacheKey, cancellationToken)) ?? false
            };
        }

        internal async Task<bool> TryValidateTokenAsync(CancellationToken cancellationToken)
        {
            if(!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new HttpBadRequestException("missing authorization header");

            return await _serviceClient.ValidateTokenAsync(authToken, cancellationToken);
        }
    }
}