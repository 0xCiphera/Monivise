using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Buckets
{
    public class BucketResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal AllocationPercent { get; set; }
        public decimal? SavingsTarget { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public decimal Allocated { get; set; }
        public decimal Spent { get; set; }
        public decimal Balance { get; set; }
        public decimal UsedPercent { get; set; }
        // Added for frontend sync
        public string BucketIcon { get; set; } = string.Empty;
    }
}
