using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IGoalRepository
    {
        Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Goal?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Goal?> GetActiveAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(Goal goal, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
