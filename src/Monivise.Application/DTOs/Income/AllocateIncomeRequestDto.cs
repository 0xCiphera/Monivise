using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Income
{
    public class AllocateIncomeRequestDto
    {
        public decimal Amount { get; set; }
        public string Source { get; set; } = "Income";
    }
}
