
using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class WantCategoryRepository : IWantCategoryRepository
    {
        private readonly AppDbContext _db;
        public WantCategoryRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<WantCategory>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _db.WantCategories
                .Where(w => w.UserId == userId && w.IsActive)
                .OrderBy(w => w.DisplayOrder)
                .ToListAsync(ct);

        public async Task<WantCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.WantCategories.FindAsync([id], ct);

        public async Task AddAsync(WantCategory wantCategory, CancellationToken ct = default)
            => await _db.WantCategories.AddAsync(wantCategory, ct);

        public async Task AddRangeAsync(IEnumerable<WantCategory> wantCategories, CancellationToken ct = default)
            => await _db.WantCategories.AddRangeAsync(wantCategories, ct);

        public async Task DeactivateAllForUserAsync(Guid userId, CancellationToken ct = default)
        {
            var active = await _db.WantCategories.Where(w => w.UserId == userId && w.IsActive).ToListAsync(ct);
            foreach (var w in active) w.Deactivate();
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
