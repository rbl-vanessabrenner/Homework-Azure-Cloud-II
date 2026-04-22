using System;
using System.Text.Json.Serialization;

namespace Library.API.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string UserRole { get; set; }
        public string Email { get; set; }
    }
}
