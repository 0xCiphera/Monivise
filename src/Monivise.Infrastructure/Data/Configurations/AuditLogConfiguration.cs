using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monivise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
            builder.Property(a => a.EntityType).HasMaxLength(100);
            builder.Property(a => a.Payload).HasColumnType("jsonb");
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.CreatedAt);
        }
    }
}
