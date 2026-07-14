using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IWantCategoryRepository
    {
        Task<IEnumerable<WantCategory>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<WantCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(WantCategory wantCategory, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
