using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
