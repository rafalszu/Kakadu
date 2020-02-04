using System;
using System.Collections.Generic;

namespace Kakadu.DTO
{
    public class ServiceDTO : IEntityDTO
    {
        public Guid Id { get; set; }
        
        public string Code { get; set; }

        public string Name { get; set; }

        public Uri Address { get; set; }

        public bool UnkownRoutesPassthrough { get; set; }

        public List<KnownRouteDTO> KnownRoutes { get; set; }
    }
}
