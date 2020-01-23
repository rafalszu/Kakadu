using System;
using System.Collections.Generic;
using SV.Core.Interfaces;

namespace SV.Core.Models
{
    public class KnownRouteModel : IModel
    {
        public Guid Id { get; set; }
        
        public string RelativeUrl { get; set; }

        public List<KnownRouteReplyModel> Replies { get; set; }

        public MethodTypeEnum Method { get; set; }

        public string Action { get; set; }
    }
}