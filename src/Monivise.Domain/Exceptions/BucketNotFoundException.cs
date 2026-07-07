using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Exceptions
{
    public class BucketNotFoundException : DomainException
    {
        public BucketNotFoundException(Guid bucketId)
            : base("BUCKET_NOT_FOUND", $"Bucket {bucketId} not found or inactive.") { }
    }
}
