using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class GoalNotActiveException : DomainException
    {
        public GoalNotActiveException()
            : base("GOAL_NOT_ACTIVE", "This goal isn't active anymore.") { }
    }
}
