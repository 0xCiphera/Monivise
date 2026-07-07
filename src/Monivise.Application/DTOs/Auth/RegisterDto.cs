using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Monivise.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [RegularExpression("^(NGN|USD|GBP)$", ErrorMessage = "Currency must be NGN, USD, or GBP")]
        public string Currency { get; set; } = "NGN";
    }
}
