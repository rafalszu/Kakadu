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
using Kakadu.Core.Interfaces;
using Kakadu.DTO;

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
        private readonly IServiceService _service;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IDistributedCache cache, 
            IActionApiHttpClient actionApiHttpClient, 
            IServiceService service,
            ILogger<RecordController> logger)
        {
            _logger = logger;
            _actionApiHttpClient = actionApiHttpClient;
            _service = service;
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
                    var results = await _actionApiHttpClient.StartRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                    return new ActionApiCallResult
                    {
                        Host = instance,
                        Result = results
                    };
                },
                cancellationToken: cancellationToken
            );

            return Ok(actionResult);
        }

        [HttpPost("stop/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResult>>> StopRecordingAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    var results = await _actionApiHttpClient.StopRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                    return new ActionApiCallResult
                    {
                        Host = instance,
                        Result = results
                    };
                },
                cancellationToken: cancellationToken
            );

            return Ok(actionResult);
        }

        [HttpGet("status/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResult>>> GetStatusAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    var results = await _actionApiHttpClient.GetStatusAsync(instance, serviceCode, accessToken, cancellationToken);
                    return new ActionApiCallResult
                    {
                        Host = instance,
                        Result = results
                    };
                },
                cancellationToken: cancellationToken
            );

            return Ok(actionResult);
        }

        [HttpGet("status")]
        public async Task<Dictionary<string, List<ActionApiCallResult>>> GetStatusesAsync(CancellationToken cancellationToken)
        {
            // get all services
            var services = _service.GetAll()?.Select(x => x.Code).ToList();
            
            var actionResult = await CallActionApiAsync(
                apiCall: async(instance, accessToken) => 
                {
                    var instanceResults = await _actionApiHttpClient.GetStatusesAsync(instance, accessToken, services, cancellationToken);
                    return instanceResults;
                },
                cancellationToken: cancellationToken
            );

            var dict = new Dictionary<string, List<ActionApiCallResult>>();
            foreach (var actionApiCallResult in actionResult)
            {
                if (!(actionApiCallResult.Result is List<ServiceCaptureStatusDTO> capturestatusList)) 
                    continue;
                
                foreach (var g in capturestatusList)
                {
                    var list = dict.ContainsKey(g.ServiceCode) ? dict[g.ServiceCode] : new List<ActionApiCallResult>();

                    list.Add(new ActionApiCallResult {Host = actionApiCallResult.Host, Result = g.IsRecording});

                    dict[g.ServiceCode] = list;
                }
            }
            
            return dict;
        }
        
        private async Task<List<ActionApiCallResult>> CallActionApiAsync<T>(Func<string, string, Task<T>> apiCall, CancellationToken cancellationToken)
        {
            var instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES, token: cancellationToken);
            if (instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new HttpBadRequestException("missing authorization header");

            var tasks = Task.WhenAll(
                instances.Select(async instance => {
                    var intanceResult = await CallActionApiAsync(async () => await GetActionApiCallResultAsync(apiCall, instance, authToken));
                    return intanceResult;
                })
            );

            var results = await tasks;

            return results.ToList();
        }

        private async Task<ActionApiCallResult> CallActionApiAsync(Func<Task<ActionApiCallResult>> func)
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

        private async Task<ActionApiCallResult> GetActionApiCallResultAsync<T>(Func<string, string, Task<T>> apiCall, string instance, string authToken)
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

            if (result is ActionApiCallResult callResult)
                return callResult;
            
            return new ActionApiCallResult
            {
                Host = instance,
                Result = result
            };
        }
    }
}