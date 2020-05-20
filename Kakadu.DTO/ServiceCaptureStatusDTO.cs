using System;
using Newtonsoft.Json;

namespace Kakadu.DTO
{
    [Serializable]
    public class ServiceCaptureStatusDTO : IEntityDTO
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string ServiceCode { get; set; }
        public bool IsRecording { get; set; }
        
    }
}