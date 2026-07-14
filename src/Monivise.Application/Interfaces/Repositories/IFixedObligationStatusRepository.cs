using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IFixedObligationStatusRepository
    {
        Task<IEnumerable<FixedObligationStatus>> GetByCycleIdAsync(Guid cycleId, CancellationToken ct = default);
        Task<FixedObligationStatus?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(IEnumerable<FixedObligationStatus> statuses, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
