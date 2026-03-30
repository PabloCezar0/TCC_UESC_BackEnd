using Rota.Domain.Common;
using Rota.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Rota.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<PaginatedResult<AuditLog>> GetPaginatedAsync(
            int page, 
            int size, 
            string? userId, 
            string? tableName, 
            string? type, 
            DateTime? startDate, 
            DateTime? endDate);
    }
}