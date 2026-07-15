using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.DTOs.Dashboard
{
    public class AllocationPreview
    {
        public Guid BucketId { get; set; }
        public string BucketName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
