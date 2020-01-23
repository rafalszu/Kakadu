using System;
using System.Collections.Generic;
using SV.Core.Interfaces;

namespace SV.Core.Models
{
    public class ServiceModel : IModel
    {
        public Guid Id { get; set; }
        
        public string Code { get; set; }

        public string Name { get; set; }

        public Uri Address { get; set; }

        public bool UnkownRoutesPassthrough { get; set; }

        public List<KnownRouteModel> KnownRoutes { get; set; }
    }
}