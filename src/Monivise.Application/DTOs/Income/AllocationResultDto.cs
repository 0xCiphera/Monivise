using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Income
{
    public class AllocationResultDto
    {
        public Guid BucketId { get; set; }
        public string BucketName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
