using Microsoft.EntityFrameworkCore;
using Rota.Domain.Common;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Infra.Data.Context; 
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rota.Infra.Data.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<AuditLog>> GetPaginatedAsync(
            int page, 
            int size, 
            string? userId, 
            string? tableName, 
            string? type, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            if (!string.IsNullOrEmpty(tableName))
            {
            
                query = query.Where(a => a.TableName.Contains(tableName));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(a => a.Type == type);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.DateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
              
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.DateTime <= end);
            }

            
            query = query.OrderByDescending(a => a.DateTime);

           
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * size)
                                   .Take(size)
                                   .ToListAsync();

            return new PaginatedResult<AuditLog>
            {
                PageNumber = page,
                PageSize = size,
                TotalItems = total,
                Items = items
            };
        }
    }
}