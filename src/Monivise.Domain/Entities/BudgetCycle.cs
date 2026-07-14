using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace Monivise.Domain.Entities
{
    public class BudgetCycle
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public CycleStatus Status { get; private set; } = CycleStatus.Active;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public decimal BufferBalance { get; private set; }

        // Navigation
        public User User { get; private set; } = null!;
        public ICollection<Transaction> Transactions { get; private set; } = [];

        protected BudgetCycle() { }

        public static BudgetCycle CreateCurrentMonth(Guid userId)
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddDays(-1);
            return new BudgetCycle { UserId = userId, StartDate = start, EndDate = end };
        }

        public static BudgetCycle Create(Guid userId, DateTime start, DateTime end)
        {
            if (end <= start) throw new ArgumentException("EndDate must be after StartDate");
            return new BudgetCycle { UserId = userId, StartDate = start, EndDate = end };
        }

        public int TotalDays => (int)(EndDate - StartDate).TotalDays + 1;

        public int ElapsedDays
        {
            get
            {
                var today = DateTime.UtcNow.Date;
                if (today < StartDate.Date) return 1;
                return Math.Max(1, (int)(today - StartDate.Date).TotalDays + 1);
            }
        }

        public int RemainingDays
        {
            get
            {
                var today = DateTime.UtcNow.Date;
                if (today > EndDate.Date) return 0;
                return Math.Max(0, (int)(EndDate.Date - today).TotalDays);
            }
        }

        public void SeedBuffer(decimal amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            BufferBalance = amount;
        }

        public void DrawFromBuffer(decimal amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount > BufferBalance) throw new InvalidOperationException("Insufficient buffer balance");
            BufferBalance -= amount;
        }

        public void AddToBuffer(decimal amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            BufferBalance += amount;
        }

        public void Close() => Status = CycleStatus.Closed;
    }
}
