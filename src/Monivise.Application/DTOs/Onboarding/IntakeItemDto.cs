using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Onboarding
{
    public class IntakeItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Need";   // Need|Want|Investment
        public string Nature { get; set; } = "Soft";      // HardFixed|Soft|Unpriced
        public decimal MonthlyAmount { get; set; }
        public bool ReserveOnly { get; set; }
    }
}
