using System;
using Newtonsoft.Json;

namespace Kakadu.DTO
{
    public class ActionApiCallResultDTO : IEntityDTO
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        
        public string Host { get; set; }

        public object Result { get; set; }
    }
}