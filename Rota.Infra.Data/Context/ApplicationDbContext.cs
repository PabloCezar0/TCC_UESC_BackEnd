using Microsoft.EntityFrameworkCore;
using Rota.Domain.Entities;

namespace Rota.Infra.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Agency> Agencies => Set<Agency>();
        public DbSet<MonthlyCommission> MonthlyCommissions => Set<MonthlyCommission>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<InvoiceFile> InvoiceFiles => Set<InvoiceFile>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);


            modelBuilder.Entity<User>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Agency>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Transaction>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<MonthlyCommission>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => i.DeletedAt == null && i.Agency.DeletedAt == null);
            modelBuilder.Entity<CommissionRule>().HasQueryFilter(x => x.DeletedAt == null);
            modelBuilder.Entity<InvoiceFile>().HasQueryFilter(f => f.DeletedAt == null && f.Invoice.DeletedAt == null && f.Invoice.Agency.DeletedAt == null);
 
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is EntityBase && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (EntityBase)entityEntry.Entity;
                var now = DateTime.UtcNow; 

                entity.UpdatedAt = now;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}