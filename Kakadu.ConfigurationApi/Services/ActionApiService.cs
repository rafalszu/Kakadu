using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.Common.Extensions;
using Kakadu.ConfigurationApi.Controllers.v1;
using Kakadu.ConfigurationApi.Interfaces;
using Kakadu.Core.Interfaces;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Kakadu.ConfigurationApi.Services
{
    public class ActionApiService : IActionApiService
    {
        private readonly IDistributedCache _cache;
        private readonly IActionApiHttpClient _actionApiHttpClient;
        private readonly IServiceService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RecordController> _logger;

        public ActionApiService(IDistributedCache cache, 
            IActionApiHttpClient actionApiHttpClient, 
            IServiceService service,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RecordController> logger)
        {
            _cache = cache;
            _actionApiHttpClient = actionApiHttpClient;
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        
        public async Task<IEnumerable<ServiceCaptureResultDTO>> GetStatusesAsync(CancellationToken cancellationToken = default)
        {
            // get all services
            var services = _service.GetAll()?.Select(x => x.Code).ToList();
            if (services == null || !services.Any())
                return Enumerable.Empty<ServiceCaptureResultDTO>();
            
            var actionResult = await CallActionApiAsync(
                apiCall: async(instance, accessToken) => 
                {
                    var instanceResults = await _actionApiHttpClient.GetStatusesAsync(instance, accessToken, services, cancellationToken);
                    return instanceResults;
                },
                cancellationToken: cancellationToken
            );

            var result = new List<ServiceCaptureResultDTO>();
            
            foreach (var actionApiCallResult in actionResult)
            {
                if (!(actionApiCallResult.Result is List<ServiceCaptureStatusDTO> capturestatusList)) 
                    continue;
                
                foreach (var g in capturestatusList)
                {
                    var entry = result.FirstOrDefault(x => x.ServiceCode == g.ServiceCode) ?? new ServiceCaptureResultDTO(g.ServiceCode);

                    entry.Results.Add(new ActionApiCallResultDTO {Host = actionApiCallResult.Host, Result = g.IsRecording});
                    
                    if (!result.Contains(entry))
                        result.Add(entry);
                }
            }
            
            return result;
        }

        public async Task<List<ActionApiCallResultDTO>> GetStatusAsync(string serviceCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    var results = await _actionApiHttpClient.GetStatusAsync(instance, serviceCode, accessToken, cancellationToken);
                    return new ActionApiCallResultDTO
                    {
                        Host = instance,
                        Result = results
                    };
                },
                cancellationToken: cancellationToken
            );

            return actionResult;
        }

        public async Task<List<ActionApiCallResultDTO>> StartRecordingAsync(string serviceCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    var results = await _actionApiHttpClient.StartRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                    return new ActionApiCallResultDTO
                    {
                        Host = instance,
                        Result = results
                    };
                },
                cancellationToken: cancellationToken
            );

            return actionResult;
        }

        public async Task<List<ActionApiCallResultDTO>> StopRecordingAsync(string serviceCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var actionResult = await CallActionApiAsync(
                apiCall: async (instance, accessToken) =>
                {
                    var results = await _actionApiHttpClient.StopRecordingAsync(instance, serviceCode, accessToken, cancellationToken);
                    return new ActionApiCallResultDTO
                    {
                        Host = instance,
                        Result = results
                    };
                },
                cancellationToken: cancellationToken
            );

            return actionResult;
        }
        
        private async Task<List<ActionApiCallResultDTO>> CallActionApiAsync<T>(Func<string, string, Task<T>> apiCall, CancellationToken cancellationToken)
        {
            var instances = await _cache.GetAsync<List<string>>(KakaduConstants.ACTIONAPI_INSTANCES, token: cancellationToken);
            if (instances == null || !instances.Any())
                throw new HttpResponseException(HttpStatusCode.UnprocessableEntity, "No Action API instances registered within Configuration API. Ensure at least Action API instance is running and can connect to Configuration API");

            if (!_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authToken))
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

        private async Task<ActionApiCallResultDTO> CallActionApiAsync(Func<Task<ActionApiCallResultDTO>> func)
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

        private async Task<ActionApiCallResultDTO> GetActionApiCallResultAsync<T>(Func<string, string, Task<T>> apiCall, string instance, string authToken)
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

            if (result is ActionApiCallResultDTO callResult)
                return callResult;
            
            return new ActionApiCallResultDTO
            {
                Host = instance,
                Result = result
            };
        }
    }
}