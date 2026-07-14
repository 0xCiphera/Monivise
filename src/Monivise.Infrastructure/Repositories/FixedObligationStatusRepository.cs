using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class FixedObligationStatusRepository : IFixedObligationStatusRepository
    {
        private readonly AppDbContext _db;
        public FixedObligationStatusRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<FixedObligationStatus>> GetByCycleIdAsync(Guid cycleId, CancellationToken ct = default)
            => await _db.FixedObligationStatuses
                .Where(f => f.BudgetCycleId == cycleId)
                .ToListAsync(ct);

        public async Task<FixedObligationStatus?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.FixedObligationStatuses.FindAsync([id], ct);

        public async Task AddAsync(IEnumerable<FixedObligationStatus> statuses, CancellationToken ct = default)
            => await _db.FixedObligationStatuses.AddRangeAsync(statuses, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
