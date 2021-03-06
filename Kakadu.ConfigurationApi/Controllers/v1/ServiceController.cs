using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kakadu.Core.Interfaces;
using Kakadu.Core.Models;
using Kakadu.DTO;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Kakadu.Common.Extensions;
using Kakadu.ConfigurationApi.Interfaces;
using Kakadu.DTO.Constants;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Authorize]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IServiceService _service;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly IActionApiService _actionApiService;

        public ServiceController(ILogger<ServiceController> logger, 
            IServiceService service,
            IDistributedCache cache,
            IActionApiService actionApiService,
            IMapper mapper)
        {
            _logger = logger;
            _service = service;
            _mapper = mapper;
            _cache = cache;
            _actionApiService = actionApiService;
        }

        [HttpGet]
        public async Task<List<ServiceDTO>> GetAsync(CancellationToken cancellationToken)
        {
            var services = await _cache.GetOrAddAsync<List<ServiceDTO>>(KakaduConstants.SERVICES, async (options) => {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

                var entities = await Task.Run(() => _service.GetAll(), cancellationToken);
                return _mapper.Map<List<ServiceDTO>>(entities);
            }, token: cancellationToken);
            
            // get recording status
            var statuses = (await _actionApiService.GetStatusesAsync(cancellationToken)).ToList();
            foreach (var serviceDto in services)
            {
                var status = statuses.FirstOrDefault(s => s.ServiceCode == serviceDto.Code);
                if (status == null)
                    continue;

                serviceDto.IsRecording = status.Results.All(r => r.Result is bool bresult && bresult == true);
            }
            
            return services;
        }

        [AllowAnonymous]
        [HttpGet("{serviceCode}")]
        public async Task<ActionResult<ServiceDTO>> GetByCodeAsync(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var dto = await _cache.GetOrAddAsync(KakaduConstants.GetServiceKey(serviceCode), async (options) => {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

                var entity = await Task.Run(() => _service.Get(serviceCode));
                if(entity == null)
                    throw new HttpNotFoundException($"No service definition found for '{serviceCode}'");

                return _mapper.Map<ServiceDTO>(entity);
            });
            
            return dto;
        }

        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<ServiceDTO>> Post(CreateServiceDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException();

            var existingService = _service.Get(dto.Code);
            if (existingService != null)
                throw new HttpResponseException($"Service with code '{dto.Code}' already exists");
            
            var model = _mapper.Map<ServiceModel>(dto);
            if(model == null)
                throw new HttpResponseException("Unable to map dto");

            model =  await Task.Run(() => _service.Create(model));

            var result = _mapper.Map<ServiceDTO>(model);

            await DropServiceCacheAsync(string.Empty);

            return Ok(result);
        }

        [HttpPatch("{serviceCode}")]
        public async Task<ActionResult<ServiceDTO>> Patch(string serviceCode, [FromBody]JsonPatchDocument<ServiceDTO> patch, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));
            if (patch == null)
                throw new ArgumentNullException(nameof(patch));

            var entity = await Task.Run(() => _service.Get(serviceCode));
            if(entity == null)
                throw new HttpNotFoundException($"No service definition found for '{serviceCode}'");

            var dto = _mapper.Map<ServiceDTO>(entity);
            patch.ApplyTo(dto);

            entity = _mapper.Map<ServiceModel>(dto);
            entity = await Task.Run(() => _service.Update(entity), cancellationToken);

            if(entity != null)
                _logger.LogInformation($"Service '{dto.Code}' updated");
            
            await DropServiceCacheAsync(dto.Code, cancellationToken);

            return dto;
        }

        [HttpDelete("{serviceCode}")]
        public async Task<IActionResult> Delete(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var entity = await Task.Run(() => _service.Get(serviceCode), cancellationToken);
            if(entity == null)
                throw new HttpNotFoundException($"No service definition found for '{serviceCode}'");

            var result = await Task.Run(() => _service.Delete(serviceCode), cancellationToken);
            if(result)
            {
                // remove from cache
                await DropServiceCacheAsync(serviceCode, cancellationToken);
                
                // stop recording
                await _actionApiService.StopRecordingAsync(serviceCode, cancellationToken);

                _logger.LogInformation($"Service '{serviceCode}' removed");
                return Ok();
            }
            else
                return StatusCode(500);
        }

        private async Task DropServiceCacheAsync(string serviceCode, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(KakaduConstants.SERVICES, cancellationToken);
            
            if(string.IsNullOrWhiteSpace(serviceCode))
                return;
            
            await _cache.RemoveAsync(KakaduConstants.GetServiceKey(serviceCode), cancellationToken);
        }
    }
}