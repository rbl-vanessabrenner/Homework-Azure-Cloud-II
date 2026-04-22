using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Common.DTOs
{
    public class AuthenticateResponseDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
