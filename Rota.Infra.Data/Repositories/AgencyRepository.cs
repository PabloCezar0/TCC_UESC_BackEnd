using Microsoft.EntityFrameworkCore;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Infra.Data.Context;

namespace Rota.Infra.Data.Repositories
{
    public class AgencyRepository : IAgencyRepository
    {
        private readonly ApplicationDbContext _context;

        public AgencyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Agency agency)
        {
            await _context.Agencies.AddAsync(agency);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Agency agency)
        {
            _context.Agencies.Update(agency);
            await _context.SaveChangesAsync();
        }

        public async Task<Agency?> GetByIdAsync(int id)
        {
            return await _context.Agencies
                .Include(a => a.CommissionRule) 
                .FirstOrDefaultAsync(a => a.Id == id);
        }

    public async Task<IEnumerable<Agency>> GetAllAsync()
    {
        return await _context.Agencies
            .Include(a => a.CommissionRule) 
            .ToListAsync();
    }

        public async Task<IEnumerable<Agency>> GetDeletedAsync()
        {
            return await _context.Agencies
                                 .IgnoreQueryFilters()
                                 .Where(a => a.IsDeleted)
                                 .ToListAsync();
        }

        public async Task<Agency?> GetByIdWithDeletedAsync(int id)
        {
            return await _context.Agencies        
                .IgnoreQueryFilters()              
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<Agency?> FindByExternalIdAsync(string externalId)
        {
            return await _context.Agencies
                .FirstOrDefaultAsync(a => a.ExternalId == externalId);
        }
        
    }
}
