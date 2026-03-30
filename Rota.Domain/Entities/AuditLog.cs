using System;

namespace Rota.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; 
        public string Type { get; set; } = string.Empty; 
        public string TableName { get; set; } = string.Empty; 
        public DateTime DateTime { get; set; } 
        public string PrimaryKey { get; set; } = string.Empty; 

        
        public string? OldValues { get; set; } 
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
    }
}