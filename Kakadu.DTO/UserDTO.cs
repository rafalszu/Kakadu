using System;

namespace Kakadu.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }
        
        public string Token { get; set; }
    }
}