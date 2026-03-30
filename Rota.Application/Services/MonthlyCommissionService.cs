using AutoMapper;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Domain.Common;
using Rota.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Rota.Application.Services
{
    public class MonthlyCommissionService : IMonthlyCommissionService
    {
        private readonly IMonthlyCommissionRepository _monthlyCommissionRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MonthlyCommissionService(IMonthlyCommissionRepository monthlyCommissionRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _monthlyCommissionRepository = monthlyCommissionRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetUserAgencyId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("AgencyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool IsAdminOrFinance()
        {
            var role = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Administrador" || role == "Financeiro";
        }
        public async Task<MonthlyCommissionDTO> GetByIdAsync(int id)
        {
            var commission = await _monthlyCommissionRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Registro de comissão não encontrado.");

            
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency != commission.AgencyId)
                {
                    throw new UnauthorizedAccessException("Acesso negado a este relatório.");
                }
            }

            return _mapper.Map<MonthlyCommissionDTO>(commission);
        }

        public async Task<PaginatedResult<MonthlyCommissionDTO>> GetPaginatedCommissionsAsync(
            int pageNumber, int pageSize, int? agencyId, int? year, int? month)
        {
            
            if (!IsAdminOrFinance())
            {
                var userAgency = GetUserAgencyId();
                if (userAgency == null) throw new UnauthorizedAccessException("Usuário sem agência.");
                
                agencyId = userAgency.Value;
            }

            var paginatedResult = await _monthlyCommissionRepository.GetPaginatedAsync(pageNumber, pageSize, agencyId, year, month);

            var commissionsDto = _mapper.Map<List<MonthlyCommissionDTO>>(paginatedResult.Items);

            return new PaginatedResult<MonthlyCommissionDTO>
            {
                TotalItems = paginatedResult.TotalItems,
                PageNumber = paginatedResult.PageNumber,
                PageSize = paginatedResult.PageSize,
                Items = commissionsDto
            };
        }
    }
}