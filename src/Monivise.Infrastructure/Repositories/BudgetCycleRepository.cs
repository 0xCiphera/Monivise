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
    public class BudgetCycleRepository : IBudgetCycleRepository
    {
        private readonly AppDbContext _db;
        public BudgetCycleRepository(AppDbContext db) => _db = db;

        public async Task<BudgetCycle?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _db.BudgetCycles
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CycleStatus.Active, ct);

        public async Task<IEnumerable<BudgetCycle>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _db.BudgetCycles
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync(ct);

        public async Task AddAsync(BudgetCycle cycle, CancellationToken ct = default)
            => await _db.BudgetCycles.AddAsync(cycle, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
