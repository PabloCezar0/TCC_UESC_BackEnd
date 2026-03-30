using System;

namespace Rota.Application.DTOs
{
    public class AuditLogDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; 
        public string Type { get; set; } = string.Empty; 
        public string TableName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string PrimaryKey { get; set; } = string.Empty;
  
        public object? OldValues { get; set; }
        public object? NewValues { get; set; }
        public object? AffectedColumns { get; set; }
    }
}