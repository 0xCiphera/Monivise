using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Dashboard
{
    public class WeeklyProjectionItem
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal ProjectedBalance { get; set; }
        public decimal ProjectedSafeToSpend { get; set; }
    }
}
