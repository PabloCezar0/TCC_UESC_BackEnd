using Rota.Domain.Entities;

namespace Rota.Domain.Interfaces
{
    public interface IAgencyRepository
    {
        Task<Agency?> GetByIdAsync(int id);
        Task<IEnumerable<Agency>> GetAllAsync();
        Task<IEnumerable<Agency>> GetDeletedAsync();
        Task AddAsync(Agency agency);
        Task UpdateAsync(Agency agency);
        Task<Agency?> GetByIdWithDeletedAsync(int id);
        Task<Agency?> FindByExternalIdAsync(string externalId);
    }
}
