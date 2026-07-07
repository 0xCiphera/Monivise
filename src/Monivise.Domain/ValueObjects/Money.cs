using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.ValueObjects
{
    public record Money(decimal Amount, string Currency)
    {
        public static Money Zero(string currency = "NGN") => new(0, currency);
        public static Money From(decimal amount, string currency) => new(amount, currency);
    }
}
