using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rota.Domain.Entities;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Value).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.IssueDate).IsRequired();
        builder.Property(i => i.ReferenceYear).IsRequired();
        builder.Property(i => i.ReferenceMonth).IsRequired();
        builder.Property(i => i.IsAnnual).IsRequired();
        builder.HasIndex(i => new { i.AgencyId, i.IssueDate });

        builder.HasOne(i => i.Agency)
               .WithMany()
               .HasForeignKey(i => i.AgencyId)
               .IsRequired();

  
        builder.HasOne(i => i.User)
               .WithMany()
               .HasForeignKey(i => i.UserId)
               .OnDelete(DeleteBehavior.Restrict); 
    }
}