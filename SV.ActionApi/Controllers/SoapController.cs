using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SV.ActionApi.Extensions;
using SV.ActionApi.Interfaces;
using SV.Core.Interfaces;
using SV.Core.Models;

namespace SV.ActionApi.Controllers
{
    [ApiController]
    [Consumes("text/xml")]
    [Route("soap/{serviceCode}")]
    public class SoapController : BaseController, IRequestProcessor
    {
        private readonly ILogger<SoapController> _logger;
        private readonly IDataProvider dataProvider;

        private string action;
        private string relativeUrl;
        private ServiceModel service;

        public SoapController(ILogger<SoapController> logger, IDataProvider dataProvider) : base(dataProvider)
        {
            _logger = logger;
            this.dataProvider = dataProvider;
        }

        [Route("{**catchAll}")]
        [HttpPost]
        public Task ProcessRequest(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));
                
            service = dataProvider.GetService(serviceCode);
            if(service == null)
            {
                _logger.LogError($"No service found by given code: {serviceCode}");
                throw new Exception($"No service found by given code: {serviceCode}");
            }

            object routeValues = null;
            if(Request.RouteValues.ContainsKey("catchAll")) {
                routeValues = Request.RouteValues["catchAll"];
            }

            relativeUrl = string.Format("/{0}{1}", routeValues ?? string.Empty, Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty);

            // Allows using several time the stream in ASP.Net Core
            Request.EnableBuffering(); 

            var body = GetRequestBodyAsync(Request.Method, Request.Body).GetAwaiter().GetResult();
            if(string.IsNullOrWhiteSpace(body))
            {
                _logger.LogInformation("Request body empty, returning BadRequest");
                throw new Exception(System.Net.HttpStatusCode.BadRequest.ToString());
            }

            _logger.LogInformation($"read request body, {body.Length} characters");

            action = GetSoapActionHeader(Request);
            _logger.LogInformation($"Read '{action}' SOAP Action");
            
            return ProxyCall(
                serviceModel: service,
                relativePath: relativeUrl,
                logger: _logger,
                interceptKnownRouteAsync: IntercepKnownRouteAsync);
        }

        private async Task<bool> IntercepKnownRouteAsync(HttpContext context)
        {
            var knownRoute = GetKnownRoute(service, relativeUrl, action);
            if(knownRoute != null) 
            {
                _logger.LogInformation($"Found known route for '{relativeUrl}', intercepting http call");
                
                var knownResponse = knownRoute.Replies?.FirstOrDefault();
                if(knownResponse != null) 
                {
                    _logger.LogInformation($"Found matching response for '{relativeUrl}' request");

                    if(knownResponse.Headers?.Any() ?? false)
                    {
                        foreach(var headerKey in knownResponse.Headers.Keys)
                            context.Response.Headers[headerKey] = knownResponse.Headers[headerKey];
                    }

                    context.Response.StatusCode = knownResponse.StatusCode;
                    context.Response.ContentType = knownResponse.ContentTypeString;
                    context.Response.ContentLength = knownResponse.ContentLength;
                    if(!string.IsNullOrWhiteSpace(knownResponse.ContentEncoding))
                        context.Response.Headers["Content-Encoding"] = knownResponse.ContentEncoding;

                    await context.Response.Body.WriteAsync(knownResponse.ContentRaw, 0, knownResponse.ContentRaw.Length);

                    return true;
                }
                
                // no replies stored, can passthrough the call?
                if(service.UnkownRoutesPassthrough)
                {
                    _logger.LogInformation($"No responses found for '{relativeUrl}', passing http call through");
                    return false;
                }

                // intercept call, return 200 with no content
                context.Response.StatusCode = 200;
                _logger.LogInformation($"No respones found for '{relativeUrl}', can't pass through as configured; returning NoContent");
                return true;
            }

            // route not known, but dont intercept and play ball
            if(service.UnkownRoutesPassthrough)
            {
                _logger.LogInformation($"No known routes found for '{relativeUrl}', passign http call through");
                return false;
            }

            // route not know, no passthrough, intercept with 404
            _logger.LogInformation($"No known routes found for '{relativeUrl}', can't pass through as configured; returning NotFound");
            context.Response.StatusCode = 404;
            return true;
        }

        private string GetSoapActionHeader(HttpRequest req)
        {
            req.Headers.TryGetValue("SOAPAction", out var actionValues);
            if(StringValues.IsNullOrEmpty(actionValues))
                throw new Exception("Missing SOAPAction header, aborting");

            string action = actionValues.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if(string.IsNullOrWhiteSpace(action))
                throw new Exception("Unable to extract SOAP Action from headers, aborting");

            return action.Sanitize();
        }

        private KnownRouteModel GetKnownRoute(ServiceModel service, string url, string action)
        {
            if(service == null)
                throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentNullException(nameof(action));

            var knownRoutes = service.KnownRoutes?.Where(r => !string.IsNullOrWhiteSpace(r.Action) && 
                                                            r.RelativeUrl.Equals(url, StringComparison.InvariantCultureIgnoreCase));
            if(knownRoutes.Any()) {
                // match action
                var actionRoute = knownRoutes.FirstOrDefault(r => r.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
                return actionRoute;
            }

            return null;
        }
    }
}