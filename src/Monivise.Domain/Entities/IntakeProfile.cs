using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class IntakeProfile
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public decimal BaselineMonthlyIncome { get; private set; }
        public PathwayType? ChosenPathway { get; private set; }
        public bool IsComplete { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<IntakeItem> Items { get; private set; } = [];

        protected IntakeProfile() { }

        public static IntakeProfile Create(Guid userId, decimal baselineIncome)
        {
            if (baselineIncome < 0) throw new ArgumentOutOfRangeException(nameof(baselineIncome));
            return new IntakeProfile { UserId = userId, BaselineMonthlyIncome = baselineIncome };
        }

        public void AddItem(IntakeItem item) => Items.Add(item);
        public void ClearItems() => Items.Clear();

        public void ChoosePathway(PathwayType pathway)
        {
            ChosenPathway = pathway; IsComplete = true; UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateBaseline(decimal income)
        {
            BaselineMonthlyIncome = income; UpdatedAt = DateTime.UtcNow;
        }
    }
}
