using Monivise.Application.DTOs.Buckets;
using Monivise.Application.DTOs.Cycles;
using Monivise.Application.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public decimal SafeToSpend { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal SpendingPace { get; set; }
        public string PaceLabel { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
        public int DaysElapsed { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalSpent { get; set; }
        public List<BucketResponseDto> Buckets { get; set; } = [];
        public List<TransactionResponseDto> Transactions { get; set; } = [];
        public CycleResponseDto? CurrentCycle { get; set; }
    }
}
