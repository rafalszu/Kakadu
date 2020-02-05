using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Interfaces;

namespace Kakadu.ActionApi.Controllers
{
    // [ApiController]
    // [Consumes("application/json")]
    // [Route("rest/{serviceCode}")]
    // public class RestController : BaseController, IRequestProcessor
    // {
    //     private readonly ILogger<RestController> _logger;
    //     private readonly IDataProvider dataProvider;

    //     private string relativeUrl;
    //     private ServiceModel service;

    //     public RestController(ILogger<RestController> logger, IDataProvider dataProvider) : base(dataProvider)
    //     {
    //         _logger = logger;
    //         this.dataProvider = dataProvider;
    //     }

    //     [HttpGet]
    //     [Route("{**catchAll}")]
    //     public Task ProcessRequest(string serviceCode)
    //     {
    //         if(string.IsNullOrWhiteSpace(serviceCode))
    //             throw new ArgumentNullException(nameof(serviceCode));
                
    //         service = dataProvider.GetService(serviceCode);
    //         if(service == null)
    //         {
    //             _logger.LogError($"No service found by given code: {serviceCode}");
    //             throw new Exception($"No service found by given code: {serviceCode}");
    //         }

    //         object routeValues = null;
    //         if(Request.RouteValues.ContainsKey("catchAll")) {
    //             routeValues = Request.RouteValues["catchAll"];
    //         }

    //         relativeUrl = string.Format("/{0}{1}", routeValues ?? string.Empty, Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty);

    //         return ProxyCall(
    //             serviceModel: service,
    //             relativePath: relativeUrl,
    //             logger: _logger,
    //             interceptKnownRouteAsync: InterceptKnownRouteAsync);
    //     }

    //     private async Task<bool> InterceptKnownRouteAsync(HttpContext context)
    //     {
    //         var knownRoute = service.KnownRoutes?.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RelativeUrl) && r.RelativeUrl.Equals(relativeUrl, StringComparison.InvariantCultureIgnoreCase));
    //         if(knownRoute != null)
    //         {
    //             _logger.LogInformation($"Found known route for '{relativeUrl}', intercepting http call");

    //             var knownResponse = knownRoute.Replies?.FirstOrDefault();
    //             if(knownResponse != null) 
    //             {
    //                 _logger.LogInformation($"Found matching response for '{relativeUrl}' request");

    //                 if(knownResponse.Headers?.Any() ?? false)
    //                 {
    //                     foreach(var headerKey in knownResponse.Headers.Keys)
    //                         context.Response.Headers[headerKey] = knownResponse.Headers[headerKey];
    //                 }

    //                 context.Response.StatusCode = knownResponse.StatusCode;
    //                 context.Response.ContentType = knownResponse.ContentTypeString;
    //                 context.Response.ContentLength = knownResponse.ContentLength;
    //                 if(!string.IsNullOrWhiteSpace(knownResponse.ContentEncoding))
    //                     context.Response.Headers["Content-Encoding"] = knownResponse.ContentEncoding;

    //                 await context.Response.Body.WriteAsync(knownResponse.ContentRaw, 0, knownResponse.ContentRaw.Length);

    //                 return true;
    //             }
                
    //             // no replies stored, can passthrough the call?
    //             if(service.UnkownRoutesPassthrough)
    //             {
    //                 _logger.LogInformation($"No responses found for '{relativeUrl}', passing http call through");
    //                 return false;
    //             }

    //             // intercept call, return 200 with no content
    //             _logger.LogInformation($"No respones found for '{relativeUrl}', can't pass through as configured; returning NoContent");
    //             context.Response.StatusCode = 200;
    //             return true;
    //         }
            
    //         // route not known, but dont intercept and play ball
    //         if(service.UnkownRoutesPassthrough)
    //         {
    //             _logger.LogInformation($"No known routes found for '{relativeUrl}', passign http call through");
    //             return false;
    //         }

    //         // route not known, no passthrough, intercept with 404
    //         _logger.LogInformation($"No known routes found for '{relativeUrl}', can't pass through as configured; returning NotFound");
    //         context.Response.StatusCode = 404;
    //         return true;
    //     }
    // }
}