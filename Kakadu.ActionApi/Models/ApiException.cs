using System;

namespace Kakadu.ActionApi.Models
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }
}