using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class Bucket
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Icon { get; private set; } = "💰";
        public string Color { get; private set; } = "#00CFA8";
        public BucketType Type { get; private set; }
        public decimal AllocationPercent { get; private set; }
        public decimal? SavingsTarget { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; private set; } = null!;

        protected Bucket() { }

        public static Bucket Create(Guid userId, string name, string icon, string color,
            BucketType type, decimal allocationPercent, int displayOrder, decimal? savingsTarget = null)
        {
            if (allocationPercent < 0 || allocationPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(allocationPercent), "Must be 0–100");

            return new Bucket
            {
                UserId = userId,
                Name = name.Trim(),
                Icon = icon,
                Color = color,
                Type = type,
                AllocationPercent = allocationPercent,
                SavingsTarget = type == BucketType.Investment ? savingsTarget : null,
                DisplayOrder = displayOrder
            };
        }

        public void Update(string name, string icon, string color,
            BucketType type, decimal allocationPercent, decimal? savingsTarget = null)
        {
            Name = name.Trim();
            Icon = icon;
            Color = color;
            Type = type;
            AllocationPercent = allocationPercent;
            SavingsTarget = savingsTarget;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
        public void SetDisplayOrder(int order) { DisplayOrder = order; UpdatedAt = DateTime.UtcNow; }
    }
}
