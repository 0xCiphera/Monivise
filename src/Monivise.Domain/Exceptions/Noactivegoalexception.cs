using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class NoActiveGoalException : DomainException
    {
        public NoActiveGoalException()
            : base("NO_ACTIVE_GOAL", "You selected Goal but have no active goal to contribute to.") { }
    }
}
