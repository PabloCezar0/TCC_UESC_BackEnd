using Rota.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rota.Domain.Interfaces
{
    public interface ICommissionRuleRepository
    {
        Task AddAsync(CommissionRule rule);
        Task<CommissionRule?> GetByIdAsync(int id);
        Task<IEnumerable<CommissionRule>> GetAllAsync();
        Task DeleteAsync(int id);
    }
}