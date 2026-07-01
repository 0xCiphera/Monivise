using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class IntakeProfileConfiguration : IEntityTypeConfiguration<IntakeProfile>
    {
        public void Configure(EntityTypeBuilder<IntakeProfile> b)
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.BaselineMonthlyIncome).HasPrecision(18, 2);
            b.Property(p => p.ChosenPathway).HasConversion<string>();
            b.HasIndex(p => p.UserId).IsUnique();
            b.HasMany(p => p.Items).WithOne(i => i.Profile)
                .HasForeignKey(i => i.IntakeProfileId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class IntakeItemConfiguration : IEntityTypeConfiguration<IntakeItem>
    {
        public void Configure(EntityTypeBuilder<IntakeItem> b)
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.Name).IsRequired().HasMaxLength(80);
            b.Property(i => i.Category).HasConversion<string>();
            b.Property(i => i.Nature).HasConversion<string>();
            b.Property(i => i.MonthlyAmount).HasPrecision(18, 2);
        }
    }
}
