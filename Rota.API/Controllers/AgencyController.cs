using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Application.Services;
using Rota.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _agencyService;
        private readonly AgencyInsertService _agencyInsertService;

        public AgencyController(IAgencyService agencyService, AgencyInsertService agencyInsertService)
        {
            _agencyService = agencyService;
            _agencyInsertService = agencyInsertService;
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        public async Task<IActionResult> Add([FromBody] AgencyRegisterDTO dto)
        {
            await _agencyService.AddAsync(dto);
            return Ok("Agência cadastrada com sucesso.");
        }

        [HttpPost("import")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        public async Task<IActionResult> ImportAgencies()
        {
            var total = await _agencyInsertService.ImportAndSaveAsync();
            if (total == 0)
            {
                return Ok("Nenhuma agência foi importada.");
            }
            return Ok($"Total de agências importadas: {total}");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        public async Task<IActionResult> Update(int id, [FromBody] AgencyRegisterDTO dto)
        {
            await _agencyService.UpdateAsync(id, dto);
            return Ok("Agência atualizada com sucesso.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        public async Task<IActionResult> SoftDelete(int id)
        {
            await _agencyService.RemoveAsync(id);
            return Ok("Agência removida com sucesso.");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgencyDTO>> GetById(int id)
        {
            var dto = await _agencyService.GetByIdAsync(id);
            return Ok(dto);
        }

        [HttpGet("ativos")]
        public async Task<ActionResult<IEnumerable<AgencyDTO>>> GetAll()
        {
            var list = await _agencyService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("removidas")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        public async Task<ActionResult<IEnumerable<AgencyDTO>>> GetDeleted()
        {
            var list = await _agencyService.GetDeletedAsync();
            return Ok(list);
        }
        [HttpPut("{id}/taxas")]
        [Authorize(Roles = "Administrador, Financeiro")]
        public async Task<IActionResult> RegisterFees(int id,
            [FromBody] AgencyFeesRegisterDTO dto)
        {
            await _agencyService.RegisterFeesAsync(id, dto);
            return Ok("Taxas registradas com sucesso.");
        }

        [HttpPost("{id}/restaurar")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        public async Task<IActionResult> Restore(int id)
        {
            await _agencyService.RestoreAsync(id);
            return Ok("Agência reativada com sucesso.");
        }

        
        [HttpPatch("{id}/commission-rule")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        [SwaggerOperation(
            Summary = "Define a regra de comissão da agência", 
            Description = "Envie o ID da regra para aplicar uma fórmula personalizada. Envie NULL para voltar ao cálculo padrão do sistema."
        )]
        public async Task<IActionResult> UpdateCommissionRule(int id, [FromBody] AgencyRuleUpdateDTO dto)
        {
            try
            {
                
                await _agencyService.UpdateCommissionRuleAsync(id, dto.CommissionRuleId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

    }
}
