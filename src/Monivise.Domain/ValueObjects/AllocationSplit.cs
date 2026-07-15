using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.ValueObjects
{
    public record AllocationSplit(Guid BucketId, decimal Amount);
}
