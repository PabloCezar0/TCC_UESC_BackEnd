using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

namespace Rota.Infra.Data.EntitiesConfiguration;

public class InvoiceFileConfiguration : IEntityTypeConfiguration<InvoiceFile>
{
    public void Configure(EntityTypeBuilder<InvoiceFile> b)
    {
        b.HasKey(f => f.Id);

        b.Property(f => f.Name)
          .IsRequired()
          .HasMaxLength(255);

        b.Property(f => f.ContentType)
          .IsRequired()
          .HasMaxLength(100);

        b.Property(f => f.Url)
          .IsRequired();

        b.HasOne(f => f.Invoice)
          .WithMany(i => i.Files)
          .HasForeignKey(f => f.InvoiceId);
    }
}
