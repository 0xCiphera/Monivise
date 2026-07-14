using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Dashboard
{
    public record DecisionSimulationResult
    {
        public string BucketName { get; init; } = string.Empty;
        public bool WillDrawFromBuffer { get; init; }  
        public decimal BufferDrawAmount { get; init; }
        public decimal BufferBalanceAfter { get; init; }
        public decimal Amount { get; init; }
        public decimal BucketBalanceBefore { get; init; }
        public decimal BucketBalanceAfter { get; init; }
        public decimal SafeToSpendBefore { get; init; }
        public decimal SafeToSpendAfter { get; init; }
        public decimal DailyLimitBefore { get; init; }
        public decimal DailyLimitAfter { get; init; }
        public decimal DepletionPercent { get; init; }
        public RiskLevel Risk { get; init; }
        public List<string> RegretSignals { get; init; } = [];
        public bool WillOverdraftBucket { get; init; }
        // Frontend-sync fields
        public decimal CurrentBalance { get; init; }
        public decimal PostSpendBalance { get; init; }
        public decimal PaceScore { get; init; }
        public decimal AverageDailySpend { get; init; }
        public int DaysRemaining { get; init; }
        public bool CanAfford { get; init; }
    }
}
