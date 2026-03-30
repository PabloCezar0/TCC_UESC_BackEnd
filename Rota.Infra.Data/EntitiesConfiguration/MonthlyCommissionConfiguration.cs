using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration
{
    public class MonthlyCommissionConfiguration : IEntityTypeConfiguration<MonthlyCommission>
    {
        public void Configure(EntityTypeBuilder<MonthlyCommission> builder)
        {
            builder.HasKey(mc => mc.Id);
            builder.Property(mc => mc.Year).IsRequired();
            builder.Property(mc => mc.Month).IsRequired();
            builder.HasIndex(mc => new { mc.AgencyId, mc.Year, mc.Month }).IsUnique();
            builder.Property(mc => mc.TotalTicketValue).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.TotalLinkValue).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.TotalInsuranceValue).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.TotalSpecialVolumeValue).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.TotalParcelValue).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.CalculatedTicketCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.CalculatedLinkCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.CalculatedInsuranceCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.CalculatedSpecialVolumeCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.CalculatedParcelCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.CalculatedCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.TotalCommission).HasColumnType("decimal(18,2)");
            builder.Property(mc => mc.TotalInvoiceDeductions).HasColumnType("decimal(18,2)");
            builder.HasOne(mc => mc.Agency).WithMany().HasForeignKey(mc => mc.AgencyId).IsRequired();
        }
    }
}