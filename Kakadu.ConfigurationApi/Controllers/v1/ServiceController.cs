using System;
using System.Collections.Generic;
using System.Net;
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

        public ServiceController(ILogger<ServiceController> logger, IServiceService service, IDistributedCache cache, IMapper mapper)
        {
            _logger = logger;
            _service = service;
            _mapper = mapper;
            _cache = cache;
        }

        [HttpGet]
        public async Task<List<ServiceDTO>> GetAsync()
        {
            var results = await _cache.GetOrAddAsync<List<ServiceDTO>>("services", async (options) => {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

                var entities = await Task.Run(() => _service.GetAll());
                return _mapper.Map<List<ServiceDTO>>(entities);
            });

            return results;
        }

        [AllowAnonymous]
        [HttpGet("{serviceCode}")]
        public async Task<ActionResult<ServiceDTO>> GetByCodeAsync(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var dto = await _cache.GetOrAddAsync(serviceCode, async (options) => {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

                var entity = await Task.Run(() => _service.Get(serviceCode));
                if(entity == null)
                    throw new HttpNotFoundException($"No service definition found for '{serviceCode}'");

                return _mapper.Map<ServiceDTO>(entity);
            });
            
            return dto;
        }

        [HttpPost("{serviceCode}")]
        [ProducesResponseType(201)]
        public async Task<ActionResult<ServiceDTO>> Post(ServiceDTO dto)
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

            return Ok(result);
        }

        [HttpPatch("{serviceCode}")]
        public async Task<ActionResult<ServiceDTO>> Patch(string serviceCode, [FromBody]JsonPatchDocument<ServiceDTO> patch)
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
            entity = await Task.Run(() => _service.Update(entity));

            return dto;
        }

        [HttpDelete("{serviceCode}")]
        public async Task<IActionResult> Delete(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var entity = await Task.Run(() => _service.Get(serviceCode));
            if(entity == null)
                throw new HttpNotFoundException($"No service definition found for '{serviceCode}'");

            var result = await Task.Run(() => _service.Delete(serviceCode));
            if(result)
            {
                // remove from cache
                _cache.Remove(serviceCode);

                _logger.LogInformation($"Service '{serviceCode}' removed");
                return Ok();
            }
            else
                return StatusCode(500);
        }
    }
}