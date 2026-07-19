using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Transactions
{
    public class RecordInvestmentDto
    {
        public Guid IntakeItemId { get; set; }
        public decimal Amount { get; set; }
    }
}
