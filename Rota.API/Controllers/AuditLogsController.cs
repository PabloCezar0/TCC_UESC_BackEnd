using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Domain.Common;
using Rota.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogsController(IAuditLogService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lista logs de auditoria do sistema",
            Description = "Permite filtrar por usuário, tabela, tipo de ação (Create/Update/Delete) e intervalo de datas."
        )]
        public async Task<ActionResult<PaginatedResult<AuditLogDTO>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? userId = null,
            [FromQuery] string? tableName = null,
            [FromQuery] string? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _service.GetLogsAsync(pageNumber, pageSize, userId, tableName, type, startDate, endDate);
            return Ok(result);
        }
    }
}