using Microsoft.EntityFrameworkCore;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rota.Infra.Data.Repositories
{
    public class CommissionRuleRepository : ICommissionRuleRepository
    {
        private readonly ApplicationDbContext _context;

        public CommissionRuleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CommissionRule rule)
        {
            await _context.CommissionRules.AddAsync(rule);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CommissionRule>> GetAllAsync()
        {
            return await _context.CommissionRules
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var rule = await GetByIdAsync(id);
            if (rule != null)
            {
                _context.CommissionRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<CommissionRule?> GetByIdAsync(int id)
        {
            return await _context.CommissionRules
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}