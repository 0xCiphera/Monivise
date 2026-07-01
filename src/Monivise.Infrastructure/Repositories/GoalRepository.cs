using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class GoalRepository(AppDbContext db) : IGoalRepository
    {
        public async Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await db.Goals.Where(g => g.UserId == userId)
                .OrderByDescending(g => g.Status == GoalStatus.Active)
                .ThenByDescending(g => g.CreatedAt).ToListAsync(ct);

        public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await db.Goals.FindAsync([id], ct);

        public async Task<Goal?> GetActiveAsync(Guid userId, CancellationToken ct = default)
            => await db.Goals.FirstOrDefaultAsync(
                g => g.UserId == userId && g.Status == GoalStatus.Active, ct);

        public async Task AddAsync(Goal goal, CancellationToken ct = default)
            => await db.Goals.AddAsync(goal, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await db.SaveChangesAsync(ct);
    }
}
