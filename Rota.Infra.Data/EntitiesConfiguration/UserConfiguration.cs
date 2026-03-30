using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
            builder.Property(p => p.Email).IsRequired();

            builder.Property(p => p.Password);

            builder.Property(p => p.AgencyId).IsRequired(false);

            builder.HasOne(u => u.Agency)
                   .WithMany()
                   .HasForeignKey(u => u.AgencyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(p => p.Role)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

        }
    }
}
