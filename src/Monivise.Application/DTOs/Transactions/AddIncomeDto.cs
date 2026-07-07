using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Monivise.Application.DTOs.Transactions
{
    public class AddIncomeDto
    {
        [Required, Range(1, 9_999_999)]
        public decimal Amount { get; set; }

        [Required, RegularExpression("^(Salary|Freelance|Gift|Business|Other)$")]
        public string Source { get; set; } = "Salary";

        [RegularExpression("^(Primary|Extra)$")]
        public string IncomeType { get; set; } = "Primary";
    }
}
