using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration
{
    public class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
    {
        public void Configure(EntityTypeBuilder<CommissionRule> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.TotalSalesFormula).HasMaxLength(500).IsRequired();
            builder.Property(x => x.CommissionFormula).HasMaxLength(500).IsRequired();
        }
    }
}