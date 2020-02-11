using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kakadu.DTO
{
    [Serializable]
    public class KnownRouteDTO : IEntityDTO
    {
        public Guid Id { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string RelativeUrl { get; set; }

        public List<KnownRouteReplyDTO> Replies { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string MethodName { get; set; }

        public string Action { get; set; }
    }
}