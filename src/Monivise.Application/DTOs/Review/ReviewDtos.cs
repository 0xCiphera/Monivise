using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Review
{
    public class FixedActualDto
    {
        public Guid IntakeItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Reserved { get; set; }
        public decimal ActualSpent { get; set; }
    }

    public class WeeklyReviewDto
    {
        public List<FixedActualDto> FixedPrompts { get; set; } = [];
        public decimal DailyUnderspend { get; set; }
        public decimal TotalSurplus { get; set; }
        public GoalRef? ActiveGoal { get; set; }
    }

    public class GoalRef
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
    }

    public class ApplySweepDto
    {
        public Guid GoalId { get; set; }
        public decimal Amount { get; set; }
        public List<FixedActualDto> FixedActuals { get; set; } = [];
    }
}
