using System;
using System.ComponentModel.DataAnnotations;

namespace Kakadu.DTO
{
    [Serializable]
    public class UserDTO : IEntityDTO
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }
        
        public string Token { get; set; }
    }
}