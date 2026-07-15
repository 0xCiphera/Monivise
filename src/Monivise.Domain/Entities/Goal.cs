using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class Goal
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Icon { get; private set; } = "🎯";
        public decimal TargetAmount { get; private set; }
        public decimal CurrentAmount { get; private set; }
        public GoalStatus Status { get; private set; } = GoalStatus.Active;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; private set; }

        protected Goal() { }

        public static Goal Create(Guid userId, string name, decimal target, string icon = "🎯")
        {
            if (target <= 0) throw new ArgumentException("Target must be positive");
            return new Goal { UserId = userId, Name = name.Trim(), TargetAmount = target, Icon = icon };
        }

        public decimal ProgressPercent =>
            TargetAmount == 0 ? 0 : Math.Min(100m, Math.Round(CurrentAmount / TargetAmount * 100m, 1));

        public void Contribute(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");
            if (Status != GoalStatus.Active) throw new InvalidOperationException("Goal is not active");
            CurrentAmount += amount;
            if (CurrentAmount >= TargetAmount)
            {
                CurrentAmount = TargetAmount;
                Status = GoalStatus.Completed;
                CompletedAt = DateTime.UtcNow;
            }
        }

        public void Pause() => Status = GoalStatus.Paused;
        public void Resume() => Status = GoalStatus.Active;
    }
}
