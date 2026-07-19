using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class FixedObligationStatusConfiguration : IEntityTypeConfiguration<FixedObligationStatus>
    {
        public void Configure(EntityTypeBuilder<FixedObligationStatus> builder)
        {
            builder.HasKey(f => f.Id);
            builder.HasIndex(f => new { f.BudgetCycleId, f.IntakeItemId }).IsUnique();
            builder.HasOne(f => f.Cycle).WithMany().HasForeignKey(f => f.BudgetCycleId);
            builder.HasOne(f => f.Item).WithMany().HasForeignKey(f => f.IntakeItemId);
        }
    }
}
