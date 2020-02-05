using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Authorize]
    public class RecordController : ControllerBase
    {
        private readonly IAppCache _cache;

        public RecordController(IAppCache cache)
        {
            _cache = cache;
        }

        [HttpPost("start/{serviceCode}")]
        public async Task<ActionResult> StartRecording(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if(instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            // TODO: send request to each action api to start reporting back routes

            return Ok();
        }

        [HttpPost("stop/{serviceCode}")]
        public async Task<ActionResult> StopRecording(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            List<string> instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES);
            if(instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            // TODO: send request to each action api to stop reporting back routes

            return Ok();
        }
    }
}