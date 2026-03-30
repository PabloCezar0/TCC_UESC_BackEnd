using AutoMapper;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Domain.Enums; 
using System;
using System.Collections.Generic;
using System.Security.Claims; 
using Microsoft.AspNetCore.Http; 
using System.Threading.Tasks;

namespace Rota.Application.Services
{
    public class AgencyService : IAgencyService
    {
        private readonly IAgencyRepository _repository;
        private readonly ICommissionRuleRepository _ruleRepository;
        private readonly IMapper _mapper;
        private readonly CommissionCalculationService _calculationService;
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public AgencyService(
            IAgencyRepository repository,
            ICommissionRuleRepository ruleRepository,
            IMapper mapper,
            CommissionCalculationService calculationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _ruleRepository = ruleRepository;
            _mapper = mapper;
            _calculationService = calculationService;
            _httpContextAccessor = httpContextAccessor;
        }



        private bool IsAdminOrFinance()
        {
            var role = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
           
            return role == "Administrador" || role == "Financeiro"; 
        }

        private int? GetUserAgencyId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("AgencyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }


        private void ValidateAccess(int targetAgencyId)
        {
            
            if (IsAdminOrFinance()) return;

            
            var userAgencyId = GetUserAgencyId();
            
            if (userAgencyId == null || userAgencyId != targetAgencyId)
            {
                
                throw new UnauthorizedAccessException("Acesso negado: Você não tem permissão para gerenciar esta agência.");
            }
        }

       

        public async Task AddAsync(AgencyRegisterDTO dto)
        {

            if (!IsAdminOrFinance())
            {
                 throw new UnauthorizedAccessException("Apenas administradores podem cadastrar novas agências.");
            }

            var agency = _mapper.Map<Agency>(dto);
            await _repository.AddAsync(agency);
        }

        public async Task UpdateAsync(int id, AgencyRegisterDTO dto)
        {
            ValidateAccess(id); 

            var agency = await _repository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Agência não encontrada.");

            agency.Update(
                dto.CorporateName,
                dto.CNPJ,
                dto.Address,
                dto.City,
                dto.State,
                dto.AddressNumber,
                dto.AddressComment,
                dto.PhoneNumberOne,
                dto.PhoneNumberTwo,
                dto.Email
            );

            await _repository.UpdateAsync(agency);
        }

        public async Task RemoveAsync(int id)
        {
          
            if (!IsAdminOrFinance())
                 throw new UnauthorizedAccessException("Apenas administradores podem remover agências.");

            var agency = await _repository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Agência não encontrada.");

            agency.SoftDelete();
            await _repository.UpdateAsync(agency);
        }

        public async Task<AgencyDTO> GetByIdAsync(int id)
        {
            ValidateAccess(id); 

            var agency = await _repository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Agência não encontrada.");
            
            return _mapper.Map<AgencyDTO>(agency);
        }

        public async Task<IEnumerable<AgencyDTO>> GetAllAsync()
        {
           
            if (IsAdminOrFinance())
            {
              
                var list = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<AgencyDTO>>(list);
            }
            else
            {
              
                var userAgencyId = GetUserAgencyId();
                if (userAgencyId.HasValue)
                {
                    var agency = await _repository.GetByIdAsync(userAgencyId.Value);
                    if (agency != null)
                    {
                        
                        return new List<AgencyDTO> { _mapper.Map<AgencyDTO>(agency) };
                    }
                }
                return new List<AgencyDTO>();
            }
        }

        public async Task<IEnumerable<AgencyDTO>> GetDeletedAsync()
        {

            if (!IsAdminOrFinance())
                 throw new UnauthorizedAccessException("Acesso negado.");

            var list = await _repository.GetDeletedAsync();
            return _mapper.Map<IEnumerable<AgencyDTO>>(list);
        }

        public async Task RegisterFeesAsync(int id, AgencyFeesRegisterDTO dto)
        {

            ValidateAccess(id); 

            var agency = await _repository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException("Agência não encontrada.");

            agency.RegisterFees(
                dto.FeePackage,
                dto.FeeTicket,
                dto.FeeInternet,
                dto.FeeInsurance,
                dto.FeeSpecialVolume,
                dto.FeeRegisteredAt
            );

            await _repository.UpdateAsync(agency);
        }

        public async Task RestoreAsync(int id)
        {
            if (!IsAdminOrFinance())
                 throw new UnauthorizedAccessException("Apenas administradores podem restaurar agências.");

            var agency = await _repository.GetByIdWithDeletedAsync(id)
                        ?? throw new KeyNotFoundException("Agência não encontrada.");

            if (!agency.IsDeleted)
                throw new InvalidOperationException("Agência já está ativa.");

            agency.Restore();
            await _repository.UpdateAsync(agency);
        }

        public async Task UpdateCommissionRuleAsync(int agencyId, int? ruleId)
        {

            if (!IsAdminOrFinance())
            {
                throw new UnauthorizedAccessException("Apenas administradores podem alterar a regra de comissão.");
            }

            var agency = await _repository.GetByIdAsync(agencyId)
                        ?? throw new KeyNotFoundException("Agência não encontrada.");

            if (ruleId.HasValue)
            {
                var ruleExists = await _ruleRepository.GetByIdAsync(ruleId.Value);
                if (ruleExists == null) 
                    throw new ArgumentException("A regra de comissão informada não existe.");
            }

            agency.SetCommissionRule(ruleId);
            await _repository.UpdateAsync(agency);

            var today = DateTime.UtcNow;
            await _calculationService.CalculateAndSaveForPeriod(today.Year, today.Month);
        }
    }
}