using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class BudgetCycleConfiguration : IEntityTypeConfiguration<BudgetCycle>
    {
        public void Configure(EntityTypeBuilder<BudgetCycle> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Status).HasConversion<string>();
            builder.HasIndex(c => new { c.UserId, c.Status });
            builder.Property(c => c.BufferBalance).HasPrecision(18, 2);
        }
    }
}
