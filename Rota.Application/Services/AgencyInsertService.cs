using Rota.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Rota.Application.Services
{
    public class AgencyInsertService
    {
        private readonly AgencyImportService _importService;
        private readonly IAgencyRepository _agencyRepository;

        public AgencyInsertService(AgencyImportService importService, IAgencyRepository agencyRepository)
        {
            _importService = importService;
            _agencyRepository = agencyRepository;
        }

        public async Task<int> ImportAndSaveAsync()
        {
            var agencies = await _importService.ImportAgenciesAsync();
            if (agencies == null || !agencies.Any())
            {
                return 0;
            }

            int savedCount = 0;
            foreach (var agency in agencies)
            {
                var existingAgency = await _agencyRepository.FindByExternalIdAsync(agency.ExternalId);

                if (existingAgency == null)
                {
                    await _agencyRepository.AddAsync(agency);
                    savedCount++;
                }
            }

            return savedCount;
        }
    }
}