using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Cycles
{
    public class CycleResponseDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
