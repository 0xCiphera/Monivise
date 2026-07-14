using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class WantCategoryConfiguration : IEntityTypeConfiguration<WantCategory>
    {
        public void Configure(EntityTypeBuilder<WantCategory> builder)
        {
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Name).IsRequired().HasMaxLength(80);
            builder.Property(w => w.MonthlyAmount).HasPrecision(18, 2);
            builder.HasIndex(w => new { w.UserId, w.IsActive });
        }
    }
}
