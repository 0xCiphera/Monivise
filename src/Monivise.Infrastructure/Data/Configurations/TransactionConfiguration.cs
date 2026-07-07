using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Amount).HasPrecision(18, 2);
            builder.Property(t => t.Kind).HasConversion<string>();
            builder.Property(t => t.IncomeType).HasConversion<string>().HasDefaultValue(IncomeType.Primary);
            builder.Property(t => t.Note).HasMaxLength(300);
            builder.Property(t => t.Source).HasMaxLength(50);

            builder.HasOne(t => t.Bucket)
                .WithMany()
                .HasForeignKey(t => t.BucketId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Cycle)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CycleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.UserId, t.CycleId });
            builder.HasIndex(t => t.Date);
        }
    }
}
