using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kakadu.ConfigurationApi.Interfaces;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Kakadu.Common.Extensions;
using Kakadu.Core.Interfaces;
using Kakadu.Core.Models;
using Kakadu.DTO;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Authorize]
    public class RecordController : ControllerBase
    {
        private readonly IActionApiService _actionApiService;
        private readonly IDistributedCache _cache;
        private readonly IServiceService _serviceService;
        private readonly IMapper _mapper;

        public RecordController(IActionApiService actionApiService, 
            IDistributedCache cache,
            IServiceService serviceService,
            IMapper mapper)
        {
            _actionApiService = actionApiService;
            _cache = cache;
            _serviceService = serviceService;
            _mapper = mapper;
        }

        [HttpPost("start/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResultDTO>>> StartRecordingAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await _actionApiService.StartRecordingAsync(serviceCode, cancellationToken);

            return Ok(actionResult);
        }

        [HttpPost("stop/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResultDTO>>> StopRecordingAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await _actionApiService.StopRecordingAsync(serviceCode, cancellationToken);

            // save captured routes
            var cachekey = KakaduConstants.GetFoundRoutesKey(serviceCode);
            var capturedRoutes = await _cache.GetAsync<List<KnownRouteDTO>>(cachekey, token: cancellationToken);
            if (capturedRoutes != null && capturedRoutes.Any())
            {
                var entities = _mapper.Map<List<KnownRouteModel>>(capturedRoutes);
                _serviceService.AddKnownRoutes(serviceCode, entities);
            }
            
            // clear cache
            await _cache.RemoveAsync(KakaduConstants.GetServiceKey(serviceCode), cancellationToken);

            return Ok(actionResult);
        }

        [HttpGet("status/{serviceCode}")]
        public async Task<ActionResult<List<ActionApiCallResultDTO>>> GetStatusAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await _actionApiService.GetStatusAsync(serviceCode, cancellationToken);

            return Ok(actionResult);
        }

        [HttpGet("status")]
        public async Task<IEnumerable<ServiceCaptureResultDTO>> GetStatusesAsync(CancellationToken cancellationToken)
        {
            var actionResult = await _actionApiService.GetStatusesAsync(cancellationToken);

            return actionResult;
        }
    }
}