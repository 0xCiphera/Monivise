using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Buckets
{
    public class UpdateBucketDto
    {
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string? Type { get; set; }
        public decimal? AllocationPercent { get; set; }
        public decimal? SavingsTarget { get; set; }
    }
}
