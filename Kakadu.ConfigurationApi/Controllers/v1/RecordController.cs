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
        public async Task<ActionResult> StartRecording(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            return await ToggleRecording(
                apiCall: async (instance, accessToken) => {
                    return await _actionApiHttpClient.StartRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                },
                cancellationToken: cancellationToken
            );
        }

        [HttpPost("stop/{serviceCode}")]
        public async Task<ActionResult> StopRecording(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            return await ToggleRecording(
                apiCall: async(instance, accessToken) => {
                    return await _actionApiHttpClient.StopRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                },
                cancellationToken: cancellationToken);
        }

        private async Task<ActionResult> ToggleRecording(Func<string, string, Task<bool>> apiCall, CancellationToken cancellationToken)
        {
            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if (instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            if(!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
                throw new HttpBadRequestException("missing authorization header");

            var tasks = Task.WhenAll(
                instances.Select(instance => 
                    CallActionApi(async () => {
                        return await apiCall(instance, authToken);
                    })
                )
            );

            List<string> errors = new List<string>();

            try
            {
                await tasks;
            }
            catch
            {
                errors.AddRange(
                    tasks.Exception?.InnerExceptions?.Select(ex => ex.Message)
                );
            }

            return Ok(errors);
        }

        private async Task<bool> CallActionApi(Func<Task<bool>> func)
        {
            if(func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to start recording: {ex.Message}");
            }
        }
    }
}