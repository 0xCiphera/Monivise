using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _db;
        public AuditLogRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(AuditLog log, CancellationToken ct = default)
            => await _db.AuditLogs.AddAsync(log, ct);

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int take = 50, CancellationToken ct = default)
            => await _db.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .ToListAsync(ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
