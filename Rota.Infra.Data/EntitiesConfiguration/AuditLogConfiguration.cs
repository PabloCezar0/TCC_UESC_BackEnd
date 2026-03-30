using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Type).IsRequired().HasMaxLength(50);
            builder.Property(a => a.TableName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.DateTime).IsRequired();
            builder.Property(a => a.PrimaryKey).IsRequired();

            
            builder.Property(a => a.OldValues).HasColumnType("longtext");
            builder.Property(a => a.NewValues).HasColumnType("longtext");
            builder.Property(a => a.AffectedColumns).HasColumnType("longtext");
        }
    }
}