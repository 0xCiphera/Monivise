using Monivise.Application.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Cycles
{
    public class RolloverRequestDto
    {
        // Required only if the outgoing cycle's surplus is > 0. Same shape and
        // validation as Extra income's split — Buffer/Wants/Goal, sums to 100.
        public List<IncomeAllocationSplitDto>? Splits { get; set; }
    }
}
