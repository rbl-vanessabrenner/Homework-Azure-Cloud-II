using System;
using System.Collections.Generic;
using System.Text;

namespace Library.API.Models
{
    public class AuthenticateResponse
    {
        public string IdToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken {  get; set; } = string.Empty;
    }
}
