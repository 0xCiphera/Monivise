using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Dashboard
{
    public class DecisionSimulationResponseDto
    {
        public string BucketName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BucketBalanceBefore { get; set; }
        public decimal BucketBalanceAfter { get; set; }
        public decimal SafeToSpendBefore { get; set; }
        public decimal SafeToSpendAfter { get; set; }
        public decimal DailyLimitBefore { get; set; }
        public decimal DailyLimitAfter { get; set; }
        public decimal DepletionPercent { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RiskMessage { get; set; } = string.Empty;
        public List<string> RegretSignals { get; set; } = [];
        public bool WillOverdraftBucket { get; set; }
        // Added for frontend sync
        public decimal CurrentBalance { get; set; }
        public decimal PostSpendBalance { get; set; }
        public decimal PaceScore { get; set; }
        public decimal AverageDailySpend { get; set; }
        public int DaysRemaining { get; set; }
        public bool CanAfford { get; set; }
    }

    
}
