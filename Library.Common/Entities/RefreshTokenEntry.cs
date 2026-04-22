using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Common.Entities
{
    public class RefreshTokenEntry
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime Expiry { get; set; }
    }
}
