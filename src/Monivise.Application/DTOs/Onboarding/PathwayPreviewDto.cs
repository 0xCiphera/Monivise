using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Onboarding
{
    public class PathwayPreviewDto
    {
        public string Pathway { get; set; } = string.Empty;
        public bool IsAffordable { get; set; }
        public decimal MonthlySavings { get; set; }
        public decimal SaveRate { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal WeeklyLimit { get; set; }
        public decimal AffordabilityGap { get; set; }     // >0 means short by this much
        public List<string> SuggestedCuts { get; set; } = [];
        public List<PathwayBucketDto> Buckets { get; set; } = [];
    }
}
