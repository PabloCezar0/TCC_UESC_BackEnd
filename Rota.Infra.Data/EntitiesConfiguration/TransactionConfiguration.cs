using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Type)
                .HasConversion<string>() 
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.Value)
                .HasColumnType("decimal(18,2)") 
                .IsRequired();
                
            builder.Property(t => t.IsManual).IsRequired(); 

            builder.Property(c => c.Date).IsRequired();
            builder.Property(c => c.TransactionIdApi).IsRequired();

            builder.HasOne(c => c.Agency)
                   .WithMany() 
                   .HasForeignKey(c => c.AgencyId)
                   .IsRequired();

            
         builder.HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.AgencyId, t.Date });
        }
    }
}