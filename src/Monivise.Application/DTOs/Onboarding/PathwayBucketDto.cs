using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Onboarding
{
    public class PathwayBucketDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;   // Fixed|Flexible|Investment|Wants
        public decimal AllocationPercent { get; set; }
    }
}
