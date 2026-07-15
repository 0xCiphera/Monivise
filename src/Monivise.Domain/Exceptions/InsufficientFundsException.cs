using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class InsufficientFundsException : DomainException
    {
        public InsufficientFundsException(string bucketName, decimal available, decimal requested)
            : base("INSUFFICIENT_FUNDS",
                $"Bucket '{bucketName}' has {available:C} available but {requested:C} requested.")
        { }
    }
}
