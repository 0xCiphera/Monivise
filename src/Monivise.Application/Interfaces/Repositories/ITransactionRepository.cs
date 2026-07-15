using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetByCycleIdAsync(Guid cycleId, CancellationToken ct = default);
        Task<IEnumerable<Transaction>> GetByUserAndCycleAsync(Guid userId, Guid cycleId, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default);
        Task AddAsync(Transaction transaction, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
