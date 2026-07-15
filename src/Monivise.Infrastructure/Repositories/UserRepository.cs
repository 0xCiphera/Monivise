using Microsoft.EntityFrameworkCore;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Users.FindAsync([id], ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
            => await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => await _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
            => await _db.Users.AddAsync(user, ct);

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
