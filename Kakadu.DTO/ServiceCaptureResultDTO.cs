using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kakadu.DTO
{
    public class ServiceCaptureResultDTO :  IEntityDTO
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        
        public string ServiceCode { get; }

        public List<ActionApiCallResultDTO> Results { get; } = new List<ActionApiCallResultDTO>();

        public int TotalInstances => Results.Count;

        public int TotalActiveCaptures => Results.Count(x => x.Result is bool b && b);

        public ServiceCaptureResultDTO(string serviceCode) => ServiceCode = serviceCode;
    }
}