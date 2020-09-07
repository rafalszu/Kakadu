using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Extensions;
using Kakadu.DTO;
using Microsoft.Extensions.Primitives;
using Kakadu.ActionApi.Interfaces;
using System.Threading;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
using Microsoft.Extensions.Caching.Distributed;
using Kakadu.Common.Extensions;

[assembly: InternalsVisibleTo("Kakadu.ActionApi.Tests")]
namespace Kakadu.ActionApi.Controllers
{
    public abstract class BaseActionApiController : ControllerBase
    {
        private readonly ILogger<BaseActionApiController> _logger;
        private readonly IAnonymousServiceHttpClient _anonymousServiceClient;
        private readonly IAuthenticatedServiceHttpClient _authenticatedServiceClient;
        private readonly IDistributedCache _cache;

        protected BaseActionApiController(ILogger<BaseActionApiController> logger, 
                                       IAnonymousServiceHttpClient anonymousServiceClient,
                                       IAuthenticatedServiceHttpClient authenticatedServiceClient,
                                       IDistributedCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _anonymousServiceClient = anonymousServiceClient ?? throw new ArgumentNullException(nameof(anonymousServiceClient));
            _authenticatedServiceClient = authenticatedServiceClient ?? throw new ArgumentNullException(nameof(authenticatedServiceClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public abstract KnownRouteDTO GetKnownRoute(ServiceDTO service, string relativePath, string action = "");

        [NonAction]
        protected async Task ProxyCall(string serviceCode, string relativePath, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(serviceCode);
            if(string.IsNullOrWhiteSpace(relativePath))
                throw new Exception($"No service found by given code: '{serviceCode}'");

            var service = await _anonymousServiceClient.GetByCodeAsync(serviceCode, cancellationToken);

            var options = ProxyOptions.Instance
                .WithHttpClientName("KakaduVirtualizationClient")
                .WithShouldAddForwardedHeaders(true)
                .WithIntercept(async (context) => await InterceptKnownRouteAsync(Request.Headers, context.Response, relativePath, service))
                .WithAfterReceive(async (context, response) => {
                    if(ShouldSaveResponse(serviceCode)) {
                        _logger.LogInformation($"Saving response from {relativePath}");

                        var dto = response.ToKnownRouteDTO();
                        await StoreReply(serviceCode, dto, cancellationToken);
                    }
                });

            var url = CombinePaths(service.Address.AbsoluteUri, relativePath);

            await this.ProxyAsync(url, options);
        }

        private async Task StoreReply(string serviceCode, KnownRouteDTO dto, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                await _authenticatedServiceClient.StoreReply(serviceCode, dto, cancellationToken); // this can throw 401 or something
                _logger.LogInformation($"{serviceCode} known routes stored");
            }
            catch(HttpResponseException ex)
            {
                _logger.LogError($"Unable to store reply: {ex.StatusCode}/{ex.Body}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private bool ShouldSaveResponse(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);
            
            return _cache.GetAsync<bool?>(recordCacheKey).GetAwaiter().GetResult() ?? false;
        }

        private async Task<bool> InterceptKnownRouteAsync(IHeaderDictionary headers, HttpResponse httpResponse, string relativePath, ServiceDTO dto)
        {
            if(httpResponse == null)
                throw new ArgumentNullException(nameof(httpResponse));
            if(string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentNullException(nameof(relativePath));
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.IsRecording)
                return false;

            var knownRoute = GetKnownRoute(dto, relativePath, GetSoapActionHeader(headers));
            if(knownRoute != null)
            {
                _logger.LogInformation($"Found known route for '{relativePath}', intercepting http call");

                var knownResponse = knownRoute.Replies?.FirstOrDefault();
                if(knownResponse != null)
                {
                    _logger.LogInformation($"Found matching response for '{relativePath}' request");

                    if(knownResponse.Headers?.Any() ?? false)
                    {
                        foreach(var headerKey in knownResponse.Headers.Keys)
                            httpResponse.Headers[headerKey] = knownResponse.Headers[headerKey];
                    }

                    httpResponse.StatusCode = knownResponse.StatusCode;
                    httpResponse.ContentType = knownResponse.ContentTypeString;
                    httpResponse.ContentLength = knownResponse.ContentLength;
                    if(!string.IsNullOrWhiteSpace(knownResponse.ContentEncoding) && httpResponse.Headers != null)
                        httpResponse.Headers["Content-Encoding"] = knownResponse.ContentEncoding;

                    if(knownResponse.ContentRaw != null)
                        await httpResponse.Body.WriteAsync(knownResponse.ContentRaw, 0, knownResponse.ContentRaw.Length);

                    return true;
                }
                
                // no replies stored, can passthrough the call?
                if(dto.UnkownRoutesPassthrough)
                {
                    _logger.LogInformation($"No responses found for '{relativePath}', passing http call through");
                    return false;
                }

                // intercept call, return 200 with no content
                _logger.LogInformation($"No respones found for '{relativePath}', can't pass through as configured; returning NoContent");
                httpResponse.StatusCode = 200;
                return true;
            }
            
            // route not known, but dont intercept and play ball
            if(dto.UnkownRoutesPassthrough)
            {
                _logger.LogInformation($"No known routes found for '{relativePath}', passign http call through");
                return false;
            }

            // route not known, no passthrough, intercept with 404
            _logger.LogInformation($"No known routes found for '{relativePath}', can't pass through as configured; returning NotFound");
            httpResponse.StatusCode = 404;
            return true;
        }

        private string CombinePaths(string s, string v)
        {
            if(string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(v))
                throw new Exception("Can't combine paths as at least one part is null or empty");

            return $"{s.TrimEnd('/')}/{v.TrimStart('/')}";
        }

        private async Task<string> GetRequestBodyAsync(string httpRequestMethod, Stream httpRequestBody)
        {
            if(string.IsNullOrWhiteSpace(httpRequestMethod))
                throw new ArgumentNullException(nameof(httpRequestMethod));
            if(httpRequestBody == null)
                throw new ArgumentNullException(nameof(httpRequestBody));
            
            if (httpRequestMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase) ||
                httpRequestMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase) ||
                httpRequestMethod.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;

            string body;

            using (var reader  = new StreamReader(
                stream: httpRequestBody,
                encoding: System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024, 
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            httpRequestBody.Position = 0;

            return body;
        }

        private string GetSoapActionHeader(IHeaderDictionary headers)
        {
            if(headers == null)
                return string.Empty;

            headers.TryGetValue("SOAPAction", out var actionValues);
            if(StringValues.IsNullOrEmpty(actionValues))
                return string.Empty;

            string action = actionValues.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if(string.IsNullOrWhiteSpace(action))
                return string.Empty;

            return action.Sanitize();
        }
    }
}