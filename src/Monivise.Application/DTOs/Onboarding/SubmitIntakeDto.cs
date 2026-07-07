using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Onboarding
{
    public class SubmitIntakeDto
    {
        public decimal BaselineIncome { get; set; }
        public List<IntakeItemDto> Items { get; set; } = [];
    }
}
