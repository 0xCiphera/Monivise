using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public Guid BucketId { get; private set; }
        public Guid CycleId { get; private set; }
        public TransactionKind Kind { get; private set; }
        public decimal Amount { get; private set; }
        public string Note { get; private set; } = string.Empty;
        public string Source { get; private set; } = string.Empty;
        public IncomeType IncomeType { get; private set; } = IncomeType.Primary;
        public DateTime Date { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation
        public Bucket Bucket { get; private set; } = null!;
        public BudgetCycle Cycle { get; private set; } = null!;

        protected Transaction() { }

        public static Transaction CreateIncome(Guid userId, Guid bucketId, Guid cycleId,
        decimal amount, string source, IncomeType incomeType = IncomeType.Primary, DateTime? date = null)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");
            return new Transaction
            {
                UserId = userId,
                BucketId = bucketId,
                CycleId = cycleId,
                Kind = TransactionKind.Income,
                Amount = amount,
                Source = source,
                IncomeType = incomeType,
                Date = date ?? DateTime.UtcNow
            };
        }

        public static Transaction CreateExpense(Guid userId, Guid bucketId, Guid cycleId,
            decimal amount, string note, DateTime? date = null)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");
            return new Transaction
            {
                UserId = userId,
                BucketId = bucketId,
                CycleId = cycleId,
                Kind = TransactionKind.Expense,
                Amount = amount,
                Note = note,
                Date = date ?? DateTime.UtcNow
            };
        }
    }
}
