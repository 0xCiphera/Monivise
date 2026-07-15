using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Monivise.Application.DTOs.Transactions
{
    public class AddIncomeDto
    {
        [Range(1, 9_999_999)]
        public decimal? Amount { get; set; }
        public string? Source { get; set; }

        [RegularExpression("^(Primary|Extra)$")]
        public string IncomeType { get; set; } = "Primary";
        public List<IncomeAllocationSplitDto>? Splits { get; set; }
    }
}
