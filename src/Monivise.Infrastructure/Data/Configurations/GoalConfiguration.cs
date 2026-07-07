using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class GoalConfiguration : IEntityTypeConfiguration<Goal>
    {
        public void Configure(EntityTypeBuilder<Goal> b)
        {
            b.HasKey(g => g.Id);
            b.Property(g => g.Name).IsRequired().HasMaxLength(80);
            b.Property(g => g.Icon).HasMaxLength(10);
            b.Property(g => g.TargetAmount).HasPrecision(18, 2);
            b.Property(g => g.CurrentAmount).HasPrecision(18, 2);
            b.Property(g => g.Status).HasConversion<string>();
            b.HasIndex(g => new { g.UserId, g.Status });
        }
    }
}
