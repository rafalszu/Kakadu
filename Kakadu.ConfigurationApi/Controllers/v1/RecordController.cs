using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ConfigurationApi.Interfaces;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
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
    public class RecordController : ControllerBase
    {
        private readonly IAppCache _cache;
        private readonly IActionApiHttpClient _actionApiHttpClient;
        private readonly ILogger<RecordController> _logger;

        public RecordController(IAppCache cache, IActionApiHttpClient actionApiHttpClient, ILogger<RecordController> logger)
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

            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if (instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            var tasks = Task.WhenAll(
                instances.Select(instance => StartRecordingOnActionApi(instance, cancellationToken))
            );

            try
            {
                await tasks;
            }
            catch
            {
                // process exception
                var x = tasks.Exception;
            }

            return Ok();
        }

        private async Task<bool> StartRecordingOnActionApi(string instance, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(instance))
                throw new ArgumentNullException(nameof(instance));

            try
            {
                return await _actionApiHttpClient.StartRecording(instance, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to start recording on node '{instance}': {ex.Message}");
            }
        }

        [HttpPost("stop/{serviceCode}")]
        public async Task<ActionResult> StopRecording(string serviceCode)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if (instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            // TODO: send request to each action api to stop reporting back routes

            return Ok();
        }
    }
}