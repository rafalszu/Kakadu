using Newtonsoft.Json;

namespace Kakadu.DTO
{
    public class ApiErrorDTO
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}