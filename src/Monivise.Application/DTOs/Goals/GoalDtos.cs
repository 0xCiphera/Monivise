using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Goals
{
    public class CreateGoalDto
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "🎯";
        public decimal TargetAmount { get; set; }
    }

    public class GoalResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ProgressPercent { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
