using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class ValidationException : DomainException
    {
        public ValidationException(string code, string message) : base(code, message) { }
    }
}
