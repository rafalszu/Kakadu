using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SV.Core.Models;

namespace SV.ActionApi.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static KnownRouteModel ToKnownRoute(this HttpResponseMessage msg)
        {
            if(msg == null)
                return null;

            KnownRouteModel result = new KnownRouteModel
            {
                Id = Guid.NewGuid(),
                Method = Enum.Parse<MethodTypeEnum>(msg.RequestMessage.Method.Method, true),
                RelativeUrl = $"{msg.RequestMessage.RequestUri.PathAndQuery}{msg.RequestMessage.RequestUri.Fragment}",
                Action = msg.RequestMessage.GetActionHeaderValue(),
                Replies = new List<KnownRouteReplyModel>
                {
                    new KnownRouteReplyModel
                    {
                        Id = Guid.NewGuid(),
                        StatusCode = (int)msg.StatusCode,
                        ContentType = msg.Content?.Headers?.ContentType?.MediaType,
                        ContentTypeCharset = msg.Content?.Headers?.ContentType?.CharSet,
                        ContentLength = msg.Content?.Headers?.ContentLength,
                        ContentBase64 = System.Convert.ToBase64String(msg.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult()),
                        ContentEncoding = msg.Content?.Headers?.ContentEncoding?.FirstOrDefault(),
                        Headers = msg.Headers?.AsDictionary()
                    }
                }
            };

            return result;
        }
    }
}