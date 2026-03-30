using Rota.Application.DTOs;
using Rota.Domain.Common;
using System;
using System.Threading.Tasks;

namespace Rota.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<PaginatedResult<AuditLogDTO>> GetLogsAsync(
            int page, 
            int size, 
            string? userId, 
            string? tableName, 
            string? type, 
            DateTime? startDate, 
            DateTime? endDate);
    }
}