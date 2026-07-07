using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IBudgetCycleRepository
    {
        Task<BudgetCycle?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<IEnumerable<BudgetCycle>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(BudgetCycle cycle, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
