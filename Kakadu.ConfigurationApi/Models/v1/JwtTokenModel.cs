using System.ComponentModel.DataAnnotations;

namespace Kakadu.ConfigurationApi.Models.V1
{
    public class JwtTokenModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}