using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Dashboard
{
    public class DecisionSimulationRequestDto
    {
        public Guid BucketId { get; set; }
        public decimal Amount { get; set; }
    }
}
