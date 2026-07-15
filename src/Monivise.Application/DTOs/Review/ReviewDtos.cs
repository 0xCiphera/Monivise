using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Review
{
    public class ItemActualDto
    {
        public Guid? IntakeItemId { get; set; }
        public Guid? WantCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Reserved { get; set; }
        public decimal Actual { get; set; }
    }

    public class WeeklyReviewDto
    {
        public bool IsMonthEnd { get; set; }
        public List<ItemActualDto> Flexible { get; set; } = [];
        public List<ItemActualDto> Investment { get; set; } = [];
        public List<ItemActualDto> WantsPriced { get; set; } = [];
        public List<ItemActualDto> FixedObligations { get; set; } = []; // only populated when IsMonthEnd
        public decimal UnpricedPoolBalance { get; set; }
        public decimal BufferBalance { get; set; }
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
    }
}
