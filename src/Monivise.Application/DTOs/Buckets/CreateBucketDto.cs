using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Monivise.Application.DTOs.Buckets
{
    public class CreateBucketDto
    {
        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Icon { get; set; } = "💰";

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Must be valid hex color e.g. #00CFA8")]
        public string Color { get; set; } = "#00CFA8";

        [Required]
        public string Type { get; set; } = "Flexible";

        [Range(0.01, 100.00)]
        public decimal AllocationPercent { get; set; }

        public decimal? SavingsTarget { get; set; }
    }
}
