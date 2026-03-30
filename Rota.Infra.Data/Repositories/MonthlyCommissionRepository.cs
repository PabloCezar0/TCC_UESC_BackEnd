using Microsoft.EntityFrameworkCore;
using Rota.Domain.Common;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Infra.Data;

namespace Rota.Infrastructure.Persistence
{
    public class MonthlyCommissionRepository : IMonthlyCommissionRepository
    {
        private readonly ApplicationDbContext _context;

        public MonthlyCommissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlyCommission?> GetByAgencyAndPeriodAsync(int agencyId, int year, int month)
        {
            return await _context.MonthlyCommissions
                .FirstOrDefaultAsync(mc => mc.AgencyId == agencyId && mc.Year == year && mc.Month == month);
        }

        public async Task AddOrUpdateAsync(MonthlyCommission monthlyCommission)
        {
            var existingCommission = await GetByAgencyAndPeriodAsync(
                monthlyCommission.AgencyId, 
                monthlyCommission.Year, 
                monthlyCommission.Month);

            if (existingCommission != null)
            {
                _context.MonthlyCommissions.Remove(existingCommission);
            }
            
            _context.MonthlyCommissions.Add(monthlyCommission);
            await _context.SaveChangesAsync();
        }
        
        public async Task<MonthlyCommission?> GetByIdAsync(int id)
        {
           
            return await _context.MonthlyCommissions
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(mc => mc.Id == id);
        }

        public async Task<PaginatedResult<MonthlyCommission>> GetPaginatedAsync(int pageNumber, int pageSize, int? agencyId, int? year, int? month)
        {
            
            var query = _context.MonthlyCommissions.Include(a => a.Agency).AsQueryable();

            
            if (agencyId.HasValue)
            {
                query = query.Where(mc => mc.AgencyId == agencyId.Value);
            }
            if (year.HasValue)
            {
                query = query.Where(mc => mc.Year == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(mc => mc.Month == month.Value);
            }

            
            var totalRecords = await query.CountAsync();

            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<MonthlyCommission>
            {
                Items = items,
                TotalItems = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}