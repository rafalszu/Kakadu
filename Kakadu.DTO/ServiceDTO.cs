using System;

namespace Kakadu.DTO
{
    public class ServiceDTO
    {
        public Guid Id { get; set; }
        
        public string Code { get; set; }

        public string Name { get; set; }

        public Uri Address { get; set; }

        public bool UnkownRoutesPassthrough { get; set; }
    }
}
