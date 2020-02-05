using System.ComponentModel.DataAnnotations;

namespace Kakadu.DTO
{
    public class JwtTokenDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}