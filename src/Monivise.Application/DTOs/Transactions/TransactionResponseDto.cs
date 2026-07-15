using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Transactions
{
    public class TransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid BucketId { get; set; }
        public string BucketName { get; set; } = string.Empty;
        public string BucketIcon { get; set; } = string.Empty;
        public string BucketColor { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string IncomeType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
