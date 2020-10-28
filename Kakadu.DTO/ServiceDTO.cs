using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kakadu.DTO
{
    [Serializable]
    public class ServiceDTO : IEntityDTO
    {
        public Guid Id { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string Code { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public Uri Address { get; set; }

        public bool UnkownRoutesPassthrough { get; set; }

        public List<KnownRouteDTO> KnownRoutes { get; set; }
        
        public List<string> Endpoints { get; set; }
        
        public bool IsRecording { get; set; }
    }
}
