using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Onboarding
{
    public class WantCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsUnpriced { get; set; }
        public decimal MonthlyAmount { get; set; }
    }
}
