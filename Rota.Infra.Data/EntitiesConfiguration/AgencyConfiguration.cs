using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration
{
     public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
     {
          public void Configure(EntityTypeBuilder<Agency> builder)
          {
               builder.HasKey(a => a.Id);

               builder.Property(a => a.ExternalId)
                    .IsRequired()
                    .HasMaxLength(50);

               builder.Property(a => a.CorporateName)
                    .IsRequired()
                    .HasMaxLength(200);

               builder.Property(a => a.CNPJ)
                    .IsRequired()
                    .HasMaxLength(18);

               builder.Property(a => a.Address)
                    .IsRequired()
                    .HasMaxLength(255);

               builder.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(100);

               builder.Property(a => a.State)
                    .IsRequired()
                    .HasMaxLength(2);

               builder.Property(a => a.AddressNumber)
                    .HasMaxLength(100);

               builder.Property(a => a.AddressComment)
                    .HasMaxLength(255);

               builder.Property(a => a.PhoneNumberOne)
                    .HasMaxLength(15);

               builder.Property(a => a.PhoneNumberTwo)
                    .HasMaxLength(15);

               builder.Property(a => a.Email)
                    .HasMaxLength(100);

               builder.Property(a => a.FeeRegisteredAt)
                   .HasColumnType("datetime")
                   .IsRequired();
                   
               builder.HasOne(a => a.CommissionRule)
                    .WithMany()
                    .HasForeignKey(a => a.CommissionRuleId)
                    .IsRequired(false) 
                    .OnDelete(DeleteBehavior.SetNull);}

     }
}
