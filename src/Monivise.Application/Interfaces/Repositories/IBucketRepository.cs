using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IBucketRepository
    {
        Task<IEnumerable<Bucket>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Bucket?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Bucket bucket, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
