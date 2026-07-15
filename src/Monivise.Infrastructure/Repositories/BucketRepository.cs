using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class BucketRepository : IBucketRepository
    {
        private readonly AppDbContext _db;
        public BucketRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Bucket>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _db.Buckets
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync(ct);

        public async Task<Bucket?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Buckets.FindAsync([id], ct);

        public async Task AddAsync(Bucket bucket, CancellationToken ct = default)
            => await _db.Buckets.AddAsync(bucket, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
