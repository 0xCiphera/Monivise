using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log, CancellationToken ct = default);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int take = 50, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
