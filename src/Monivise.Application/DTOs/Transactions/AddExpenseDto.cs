using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Monivise.Application.DTOs.Transactions
{
    public class AddExpenseDto
    {
        [Required]
        public Guid BucketId { get; set; }

        [Required, Range(1, 9_999_999)]
        public decimal Amount { get; set; }

        [MaxLength(300)]
        public string Note { get; set; } = string.Empty;
    }
}
