using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class WantCategory
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool IsUnpriced { get; private set; }
        public decimal MonthlyAmount { get; private set; } // 0 when IsUnpriced
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; private set; } = null!;

        protected WantCategory() { }

        public static WantCategory Create(Guid userId, string name, bool isUnpriced, decimal monthlyAmount, int displayOrder)
        {
            if (!isUnpriced && monthlyAmount <= 0)
                throw new ArgumentException("Priced want categories must have a positive amount");
            return new WantCategory
            {
                UserId = userId,
                Name = name.Trim(),
                IsUnpriced = isUnpriced,
                MonthlyAmount = isUnpriced ? 0 : monthlyAmount,
                DisplayOrder = displayOrder
            };
        }

        public void Deactivate() => IsActive = false;
    }
}
