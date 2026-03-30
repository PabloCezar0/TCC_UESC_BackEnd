using AutoMapper;
using Microsoft.Extensions.Logging;
using Rota.Application.Common.Storage;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Domain.Entities;
using Rota.Domain.Interfaces;
using Rota.Domain.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Rota.Domain.Enums;

namespace Rota.Application.Services;

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repo;
    private readonly IStorageService _storage;
    private readonly IMapper _mapper;
    private readonly ILogger<InvoiceService> _log;
    private readonly IMonthlyCommissionRepository _monthlyCommissionRepository;
    private readonly CommissionCalculationService _commissionCalculationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly HashSet<string> _allowed =
        new(StringComparer.OrdinalIgnoreCase) { "application/pdf" };

    public InvoiceService(        
        IInvoiceRepository repo,
        IStorageService storage,
        IMapper mapper,
        ILogger<InvoiceService> log,
        IMonthlyCommissionRepository monthlyCommissionRepository,
        CommissionCalculationService commissionCalculationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _storage = storage;
        _mapper = mapper;
        _log = log;
        _monthlyCommissionRepository = monthlyCommissionRepository;
        _commissionCalculationService = commissionCalculationService;
        _httpContextAccessor = httpContextAccessor;
    }


    private bool IsAdminOrFinance()
    {
        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        return roleClaim == UserRole.Administrador.ToString() || 
               roleClaim == UserRole.Financeiro.ToString();
    }

    private int? GetUserAgencyId()
    {

        var agencyClaim = _httpContextAccessor.HttpContext?.User.FindFirst("AgencyId")?.Value;
        return int.TryParse(agencyClaim, out var id) ? id : null;
    }


    public async Task<InvoiceDTO> UploadAsync(InvoiceUploadDTO dto, CancellationToken ct = default)
    {
        if (dto.Content == null || dto.Content.Length == 0) throw new ArgumentException("Arquivo obrigatório.", nameof(dto.Content));
        if (!_allowed.Contains(dto.ContentType)) throw new ArgumentException("Tipo de arquivo não suportado.", nameof(dto.ContentType));

        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
             throw new UnauthorizedAccessException("Usuário não identificado.");
        }


        if (!IsAdminOrFinance())
        {
            var userAgency = GetUserAgencyId();
            if (userAgency.HasValue && dto.AgencyId != userAgency.Value)
            {
                throw new UnauthorizedAccessException("Você não pode enviar notas para outra agência.");
            }
        }

        var referenceYear = dto.IssueDate.Year;
        var referenceMonth = dto.IssueDate.Month;
       
        var key = $"invoices/{referenceYear}/{referenceMonth}/{Guid.NewGuid()}_{dto.FileName}";
        var url = await _storage.UploadAsync(dto.Content, key, dto.ContentType, ct);
       

        var invoice = new Invoice(dto.Name, dto.Description, dto.Value, dto.IssueDate, referenceYear, referenceMonth, dto.AgencyId, dto.IsAnnual, userId);
       
        var file = new InvoiceFile(dto.FileName, dto.ContentType, DateTime.UtcNow, url, invoice.Id);      
        invoice.Files.Add(file);

        await _repo.AddAsync(invoice);
        
        await TriggerRecalculationIfNeededAsync(invoice.AgencyId, invoice.ReferenceYear, invoice.ReferenceMonth);

        return _mapper.Map<InvoiceDTO>(invoice);
    }

    public async Task UpdateAsync(int id, InvoiceUpdateDTO dto)
    {
        var invoice = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"NF {id} não encontrada.");
        

        if (!IsAdminOrFinance())
        {
            var userAgency = GetUserAgencyId();
            if (userAgency != invoice.AgencyId)
                throw new UnauthorizedAccessException("Acesso negado a esta nota fiscal.");
        }

        var oldAgencyId = invoice.AgencyId;
        var oldYear = invoice.ReferenceYear;
        var oldMonth = invoice.ReferenceMonth;

        invoice.Update(dto.Name, dto.Description, dto.Value, dto.IssueDate, dto.IsAnnual);
        
        await _repo.UpdateAsync(invoice);

        await TriggerRecalculationIfNeededAsync(invoice.AgencyId, invoice.ReferenceYear, invoice.ReferenceMonth);

        if(oldAgencyId != invoice.AgencyId || oldYear != invoice.ReferenceYear || oldMonth != invoice.ReferenceMonth)
        {
            await TriggerRecalculationIfNeededAsync(oldAgencyId, oldYear, oldMonth);
        }
    }

    public async Task<InvoiceFileDownloadDTO> DownloadAsync(int invoiceId, CancellationToken ct = default)
    {
        var invoice = await _repo.GetByIdWithFilesAsync(invoiceId)
                      ?? throw new KeyNotFoundException($"NF {invoiceId} não encontrada.");


        if (!IsAdminOrFinance())
        {
            var userAgency = GetUserAgencyId();
            if (userAgency != invoice.AgencyId)
                throw new UnauthorizedAccessException("Acesso negado ao arquivo.");
        }

        var file = invoice.Files.FirstOrDefault()
                   ?? throw new InvalidOperationException("NF não possui arquivo anexado.");

        string url = file.Url;
        int protocolSeparatorIndex = url.IndexOf("://");
        int pathStartIndex = url.IndexOf('/', protocolSeparatorIndex + 3); 
        
        var key = url.Substring(pathStartIndex + 1);
        
        var stream = await _storage.DownloadAsync(key, ct);

        return new InvoiceFileDownloadDTO
        {
            Content     = stream,
            ContentType = file.ContentType,
            FileName    = file.Name
        };
    }

    private async Task TriggerRecalculationIfNeededAsync(int agencyId, int year, int month)
    {
        var existingCommission = await _monthlyCommissionRepository.GetByAgencyAndPeriodAsync(agencyId, year, month);
        if (existingCommission != null)
        {
            _log.LogInformation("Recalculando comissões para Agência {AgencyId} para o período {Month}/{Year}.", agencyId, month, year);
            await _commissionCalculationService.CalculateAndSaveForPeriod(year, month, agencyId);
        }
    }

public async Task<IEnumerable<InvoiceDTO>> GetAllAsync()
    {
        if (IsAdminOrFinance())
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<InvoiceDTO>>(list);
        }
        else
        {
            
            var userAgencyId = GetUserAgencyId();
            if (userAgencyId == null) throw new UnauthorizedAccessException("Usuário sem agência vinculada.");

   
            var list = await _repo.GetByAgencyAsync(userAgencyId.Value); 
            return _mapper.Map<IEnumerable<InvoiceDTO>>(list);
        }
    }


    public async Task<PaginatedResult<InvoiceDTO>> GetPaginatedAsync(
        int page, int size,
        CancellationToken ct = default)
    {

        
        int? filterAgencyId = null;

        if (!IsAdminOrFinance())
        {
            filterAgencyId = GetUserAgencyId();
            if (filterAgencyId == null) throw new UnauthorizedAccessException("Usuário sem agência vinculada.");
        }

        var slice = await _repo.GetPaginatedAsync(page, size, filterAgencyId, null, ct);

        return new PaginatedResult<InvoiceDTO>
        {
            PageNumber = slice.PageNumber,
            PageSize   = slice.PageSize,
            TotalItems = slice.TotalItems,
            Items      = _mapper.Map<List<InvoiceDTO>>(slice.Items)
        };
    }

    public async Task<InvoiceDTO> GetByIdAsync(int id)
    {
        var inv = await _repo.GetByIdAsync(id)
                  ?? throw new KeyNotFoundException($"NF {id} não encontrada.");
        
        
        if (!IsAdminOrFinance())
        {
            var userAgency = GetUserAgencyId();
            if (userAgency != inv.AgencyId)
                throw new UnauthorizedAccessException("Acesso negado.");
        }

        return _mapper.Map<InvoiceDTO>(inv);
    }

    public async Task RemoveAsync(int id)
    {
        var invoice = await _repo.GetByIdWithFilesAsync(id) ?? throw new KeyNotFoundException($"NF {id} não encontrada.");

        
        if (!IsAdminOrFinance())
        {
            var userAgency = GetUserAgencyId();
            if (userAgency != invoice.AgencyId)
                throw new UnauthorizedAccessException("Acesso negado ao excluir.");
        }

        foreach (var file in invoice.Files.ToList())
        {
            try
            {
                string url = file.Url;
                int protocolSeparatorIndex = url.IndexOf("://");
                int pathStartIndex = url.IndexOf('/', protocolSeparatorIndex + 3); 
                var key = url.Substring(pathStartIndex + 1);

                await _storage.DeleteAsync(key);
                _log.LogInformation("Arquivo {Key} removido com sucesso do S3.", key);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Falha ao remover o arquivo do S3 para a NF {InvoiceId}.", id);
            }
        }
        
        await _repo.RemoveAsync(id);
        
        await TriggerRecalculationIfNeededAsync(invoice.AgencyId, invoice.ReferenceYear, invoice.ReferenceMonth);
    }
}