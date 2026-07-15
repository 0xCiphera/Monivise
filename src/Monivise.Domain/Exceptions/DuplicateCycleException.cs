using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class DuplicateCycleException : DomainException
    {
        public DuplicateCycleException()
            : base("DUPLICATE_CYCLE", "An active budget cycle already exists for this user.") { }
    }
}
