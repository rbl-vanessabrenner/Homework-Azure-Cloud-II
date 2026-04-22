using System;
using System.Collections.Generic;
using System.Text;

namespace Library.API.Models
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
