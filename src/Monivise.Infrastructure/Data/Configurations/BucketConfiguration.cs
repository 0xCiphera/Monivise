using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class BucketConfiguration : IEntityTypeConfiguration<Bucket>
    {
        public void Configure(EntityTypeBuilder<Bucket> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(80);
            builder.Property(b => b.Icon).HasMaxLength(10);
            builder.Property(b => b.Color).HasMaxLength(7);
            builder.Property(b => b.Type).HasConversion<string>();
            builder.Property(b => b.AllocationPercent).HasPrecision(5, 2);
            builder.Property(b => b.SavingsTarget).HasPrecision(18, 2);
            builder.HasIndex(b => new { b.UserId, b.IsActive });
        }
    }
}
