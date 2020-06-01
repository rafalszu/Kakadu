using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using Kakadu.DTO;

namespace Kakadu.ActionApi.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static KnownRouteDTO ToKnownRouteDTO(this HttpResponseMessage msg)
        {
            if(msg == null)
                return null;

            var content = msg.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            
            var result = new KnownRouteDTO
            {
                Id = Guid.NewGuid(),
                MethodName = msg.RequestMessage.Method.Method,
                RelativeUrl = $"{msg.RequestMessage.RequestUri.PathAndQuery}{msg.RequestMessage.RequestUri.Fragment}",
                Action = msg.RequestMessage.GetActionHeaderValue(),
                Replies = new List<KnownRouteReplyDTO>
                {
                    new KnownRouteReplyDTO
                    {
                        Id = Guid.NewGuid(),
                        StatusCode = (int)msg.StatusCode,
                        ContentType = msg.Content?.Headers?.ContentType?.MediaType,
                        ContentTypeCharset = msg.Content?.Headers?.ContentType?.CharSet,
                        ContentLength = msg.Content?.Headers?.ContentLength,
                        ContentBase64 = System.Convert.ToBase64String(content),
                        ContentEncoding = msg.Content?.Headers?.ContentEncoding?.FirstOrDefault(),
                        Headers = msg.Headers?.AsDictionary()
                    }
                }
            };

            return result;
        }
    }
}