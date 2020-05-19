using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ConfigurationApi.Interfaces;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Kakadu.Common.Extensions;
using Kakadu.ConfigurationApi.Models;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Authorize]
    public class RecordController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly IActionApiHttpClient _actionApiHttpClient;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IDistributedCache cache, IActionApiHttpClient actionApiHttpClient, ILogger<RecordController> logger)
        {
            _logger = logger;
            _actionApiHttpClient = actionApiHttpClient;
            _cache = cache;
        }

        [HttpPost("start/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResult>>> StartRecordingAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) => 
                {
                    return await _actionApiHttpClient.StartRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                },
                cancellationToken: cancellationToken
            );

            return actionResult?.Value?.ToList();
        }

        [HttpPost("stop/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResult>>> StopRecordingAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    return await _actionApiHttpClient.StopRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                },
                cancellationToken: cancellationToken
            );

            return actionResult?.Value?.ToList();
        }

        [HttpGet("status/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResult>>> GetStatusAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    return await _actionApiHttpClient.GetStatusAsync(instance, serviceCode, accessToken, cancellationToken);
                },
                cancellationToken: cancellationToken
            );

            return actionResult?.Value?.ToList();
        }

        [HttpGet("status")]
        public async Task<Dictionary<string, List<ActionApiCallResult>>> GetStatusesAsync(CancellationToken cancellationToken)
        {
            var actionResult = await CallActionApiAsync(
                apiCall: async(instance, accessToken) => 
                {
                    // return await _actionApiHttpClient.GetStatusesAsync(instance, accessToken, cancellationToken);
                    var instanceResults = await _actionApiHttpClient.GetStatusesAsync(instance, accessToken, cancellationToken);
                    return new ActionApiCallResult {
                        Host = instance,
                        Result = instanceResults
                    };
                },
                cancellationToken: cancellationToken
            );

            // todo: WTF
            //actionResult.Value[0].

            // transform results to dictionary
            // if(result != null && result.Value != null && result.Value.Any())
            // {
            //     result.Value.Select(v => v.)
            // }
            
            return null;
            // query action apis for all
        }


        //internal async Task<ActionResult<T>> CallActionApiAsync<T>(Func<string, string, Task<T>> apiCall, CancellationToken cancellationToken)
        internal async Task<ActionResult<ActionApiCallResult[]>> CallActionApiAsync<T>(Func<string, string, Task<T>> apiCall, CancellationToken cancellationToken)
        {
            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if (instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new HttpBadRequestException("missing authorization header");

            // var tasks = Task.WhenAll(
            //     instances.Select(instance =>
            //         CallActionApiAsync(async () => await GetActionApiCallResultAsync(apiCall, instance, authToken))
            //         // CallActionApiAsync(async () => 
            //         // {
            //         //     return await GetActionApiCallResultAsync(apiCall, instance, authToken);
            //         // })
            //     )
            // );
            var tasks = Task.WhenAll(
                instances.Select(async instance => {
                    var intanceResult = await CallActionApiAsync(async () => await GetActionApiCallResultAsync(apiCall, instance, authToken));
                    return new ActionApiCallResult {
                        Host = instance,
                        Result = intanceResult
                    };
                })
            );

            var results = await tasks;

            return Ok(results);
        }

        internal async Task<ActionApiCallResult> CallActionApiAsync(Func<Task<ActionApiCallResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                return await func();

            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to execute action api method: {ex.Message}");
            }
        }

        internal async Task<ActionApiCallResult> GetActionApiCallResultAsync<T>(Func<string, string, Task<T>> apiCall, string instance, string authToken)
        {
            object result = null;
            try
            {
                result = await apiCall(instance, authToken);
            }
            catch (Exception ex)
            {
                result = ex.InnerException?.Message ?? ex.Message;
            }

            return new ActionApiCallResult
            {
                Host = instance,
                Result = result
            };
        }
    }
}