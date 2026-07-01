using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Application.Interfaces.Repositories
{
    public interface IIntakeProfileRepository
    {
        Task<IntakeProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(IntakeProfile profile, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
