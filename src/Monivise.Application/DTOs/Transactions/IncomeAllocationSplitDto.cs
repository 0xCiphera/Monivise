using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Transactions
{
    public class IncomeAllocationSplitDto
    {
        public string Destination { get; set; } = string.Empty; // Buffer|Wants|Goal
        public decimal Percent { get; set; }
    }
}
