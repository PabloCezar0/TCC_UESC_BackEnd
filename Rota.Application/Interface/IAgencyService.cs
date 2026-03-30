using Rota.Application.DTOs;

namespace Rota.Application.Interfaces
{
    public interface IAgencyService
    {
        Task AddAsync(AgencyRegisterDTO dto);
        Task UpdateAsync(int id, AgencyRegisterDTO dto);
        Task RemoveAsync(int id);
        Task<AgencyDTO> GetByIdAsync(int id);
        Task<IEnumerable<AgencyDTO>> GetAllAsync();
        Task<IEnumerable<AgencyDTO>> GetDeletedAsync();
        Task RegisterFeesAsync(int id, AgencyFeesRegisterDTO dto);
        Task RestoreAsync(int id);
        Task UpdateCommissionRuleAsync(int agencyId, int? ruleId);

    }
}
