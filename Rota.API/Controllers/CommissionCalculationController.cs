using Microsoft.AspNetCore.Mvc;
using Rota.Application.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommissionCalculationController : ControllerBase
    {
        private readonly CommissionCalculationService _calculationService;

        public CommissionCalculationController(CommissionCalculationService calculationService)
        {
            _calculationService = calculationService;
        }

        [HttpPost("generate-monthly-report")]
        [Authorize(Roles = "Administrador, Financeiro")]
        public async Task<IActionResult> GenerateMonthlyReport(
            [FromQuery, Required, Range(2020, 2100)] int year,
            [FromQuery, Required, Range(1, 12)] int month,
            [FromQuery] int? agencyId = null)
        {
            try
            {
                await _calculationService.CalculateAndSaveForPeriod(year, month, agencyId);
                return Ok(new { message = $"Pré-cálculo de comissões para {month:00}/{year} foi gerado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocorreu um erro ao processar o cálculo das comissões.", details = ex.Message });
            }
        }
    }
}