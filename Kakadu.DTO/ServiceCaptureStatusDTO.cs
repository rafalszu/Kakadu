using System;

namespace Kakadu.DTO
{
    [Serializable]
    public class ServiceCaptureStatusDTO : IEntityDTO
    {
        public Guid Id { get; set; }
        public string ServiceCode { get; set; }
        public bool IsRecording { get; set; }
        
    }
}