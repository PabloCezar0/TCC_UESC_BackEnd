using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rota.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace Rota.Infra.Data.Interceptors
{
    public class AuditingInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditingInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;
            if (dbContext == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Sistema";

            var auditEntries = new List<AuditEntry>();

         
            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name; 
                auditEntry.UserId = userId;
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                      
                        continue;
                    }

                    string propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = "Create";
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = "Delete";
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.AuditType = "Update";
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                auditEntry.ChangedColumns.Add(propertyName);
                            }
                            break;
                    }
                }
            }

          
            foreach (var auditEntry in auditEntries)
            {
                dbContext.Add(auditEntry.ToAuditLog());
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

   
    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<string> ChangedColumns { get; } = new();
        public string AuditType { get; set; }

        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public AuditLog ToAuditLog()
        {
            var audit = new AuditLog();
            audit.UserId = UserId;
            audit.Type = AuditType;
            audit.TableName = TableName;
            audit.DateTime = DateTime.UtcNow;
            audit.PrimaryKey = JsonSerializer.Serialize(KeyValues);
            audit.OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues);
            audit.NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues);
            audit.AffectedColumns = ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns);
            return audit;
        }
    }
}