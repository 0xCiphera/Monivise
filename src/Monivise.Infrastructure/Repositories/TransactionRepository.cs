using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;
        public TransactionRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Transaction>> GetByCycleIdAsync(Guid cycleId, CancellationToken ct = default)
            => await _db.Transactions
                .Include(t => t.Bucket)
                .Where(t => t.CycleId == cycleId)
                .OrderByDescending(t => t.Date)
                .ToListAsync(ct);

        public async Task<IEnumerable<Transaction>> GetByUserAndCycleAsync(
            Guid userId, Guid cycleId, CancellationToken ct = default)
            => await _db.Transactions
                .Where(t => t.UserId == userId && t.CycleId == cycleId)
                .ToListAsync(ct);

        public async Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default)
            => await _db.Transactions.AddRangeAsync(transactions, ct);

        public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
            => await _db.Transactions.AddAsync(transaction, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
