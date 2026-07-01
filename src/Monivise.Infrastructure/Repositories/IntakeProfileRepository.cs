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
    }
}
