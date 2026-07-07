using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class IntakeProfileRepository(AppDbContext db) : IIntakeProfileRepository
    {
        public async Task<IntakeProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await db.IntakeProfiles.Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        public async Task AddAsync(IntakeProfile profile, CancellationToken ct = default)
            => await db.IntakeProfiles.AddAsync(profile, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await db.SaveChangesAsync(ct);

        public Task DeleteItemsByProfileIdAsync(Guid profileId, CancellationToken ct = default)
            => db.IntakeItems.Where(i => i.IntakeProfileId == profileId).ExecuteDeleteAsync(ct);
        public void DetachItems(IEnumerable<IntakeItem> items)
        {
            foreach (var item in items)
                db.Entry(item).State = EntityState.Detached;
        }
    }
}
