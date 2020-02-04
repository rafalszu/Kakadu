using System;
using System.Collections.Generic;

namespace Kakadu.DTO
{
    public class KnownRouteDTO : IEntityDTO
    {
        public Guid Id { get; set; }
        
        public string RelativeUrl { get; set; }

        public List<KnownRouteReplyDTO> Replies { get; set; }

        public string MethodName { get; set; }

        public string Action { get; set; }
    }
}