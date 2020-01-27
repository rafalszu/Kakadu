using System;
using System.Collections.Generic;
using Kakadu.Core.Interfaces;

namespace Kakadu.Core.Models
{
    public class KnownRouteReplyModel : IModel
    {
        public Guid Id { get; set; }
        public string ContentType { get; set; }
        public string ContentTypeCharset { get; set; }
        public long? ContentLength { get; set; }
        public string ContentBase64 { get; set; }
        public string ContentEncoding { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public byte[] ContentRaw => string.IsNullOrEmpty(ContentBase64) ? null : Convert.FromBase64String(ContentBase64);

        public string ContentTypeString => string.Format("{0}{1}{2}", 
                                                            ContentType,
                                                            (!string.IsNullOrWhiteSpace(ContentTypeCharset)) ? "; " : string.Empty,
                                                            (!string.IsNullOrWhiteSpace(ContentTypeCharset)) ? $"charset={ContentTypeCharset}" : string.Empty
                                                        );
    }
}