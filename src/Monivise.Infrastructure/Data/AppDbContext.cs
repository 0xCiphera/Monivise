using Microsoft.EntityFrameworkCore;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<BudgetCycle> BudgetCycles => Set<BudgetCycle>();
        public DbSet<Bucket> Buckets => Set<Bucket>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<IntakeProfile> IntakeProfiles => Set<IntakeProfile>();
        public DbSet<IntakeItem> IntakeItems => Set<IntakeItem>();
        public DbSet<Goal> Goals => Set<Goal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
