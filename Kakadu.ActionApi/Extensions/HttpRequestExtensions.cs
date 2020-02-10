using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kakadu.ActionApi.Extensions
{
    public static class HttpRequestExtensions 
    { 
        public static string GetIpAddress(this HttpRequest request) 
        { 
            StringValues header = StringValues.Empty;
            if(request.Headers.TryGetValue("CF-Connecting-IP", out header))
                return header.ToString();

            if(request.Headers.TryGetValue("X-Forwarded-For", out header))
                return header.ToString();
                
            return request.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}