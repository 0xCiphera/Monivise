using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class FixedObligationStatus
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid BudgetCycleId { get; private set; }
        public Guid IntakeItemId { get; private set; }
        public bool IsPaid { get; private set; }
        public DateTime? PaidAt { get; private set; }

        // Navigation
        public BudgetCycle Cycle { get; private set; } = null!;
        public IntakeItem Item { get; private set; } = null!;

        protected FixedObligationStatus() { }

        public static FixedObligationStatus Create(Guid cycleId, Guid intakeItemId) =>
            new() { BudgetCycleId = cycleId, IntakeItemId = intakeItemId };

        public void MarkPaid()
        {
            IsPaid = true;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkUnpaid()
        {
            IsPaid = false;
            PaidAt = null;
        }
    }
}
