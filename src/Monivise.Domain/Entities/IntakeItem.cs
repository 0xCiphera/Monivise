using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class IntakeItem
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid IntakeProfileId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public ExpenseCategory Category { get; private set; }
        public ItemNature Nature { get; private set; }
        public decimal MonthlyAmount { get; private set; }
        public bool ReserveOnly { get; private set; }

        public IntakeProfile Profile { get; private set; } = null!;

        protected IntakeItem() { }

        public static IntakeItem Create(Guid profileId, string name, ExpenseCategory category,
            ItemNature nature, decimal monthlyAmount, bool reserveOnly = false)
        {
            if (monthlyAmount < 0) throw new ArgumentOutOfRangeException(nameof(monthlyAmount));
            return new IntakeItem
            {
                IntakeProfileId = profileId,
                Name = name.Trim(),
                Category = category,
                Nature = nature,
                MonthlyAmount = nature == ItemNature.Unpriced ? 0 : monthlyAmount,
                ReserveOnly = reserveOnly
            };
        }
    }
}
