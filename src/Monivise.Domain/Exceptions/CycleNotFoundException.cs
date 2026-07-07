using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class CycleNotFoundException : DomainException
    {
        public CycleNotFoundException(Guid userId)
            : base("NO_ACTIVE_CYCLE", $"No active budget cycle found for user {userId}.") { }
    }
}
