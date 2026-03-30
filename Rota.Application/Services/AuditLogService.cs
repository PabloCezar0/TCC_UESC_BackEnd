using AutoMapper;
using Microsoft.AspNetCore.Http;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Domain.Common;
using Rota.Domain.Enums;
using Rota.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rota.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(
            IAuditLogRepository repository, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsAdmin()
        {
            var role = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return role == nameof(UserRole.Administrador);
        }

        public async Task<PaginatedResult<AuditLogDTO>> GetLogsAsync(
            int page, int size, string? userId, string? tableName, string? type, DateTime? startDate, DateTime? endDate)
        {
            
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Apenas administradores podem acessar logs de auditoria.");
            }

            var result = await _repository.GetPaginatedAsync(page, size, userId, tableName, type, startDate, endDate);

            var dtos = _mapper.Map<List<AuditLogDTO>>(result.Items);

            return new PaginatedResult<AuditLogDTO>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                Items = dtos
            };
        }
    }
}