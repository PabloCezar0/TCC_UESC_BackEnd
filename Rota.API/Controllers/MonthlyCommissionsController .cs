using Microsoft.AspNetCore.Mvc;
using Rota.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MonthlyCommissionsController : ControllerBase
    {
        private readonly IMonthlyCommissionService _commissionService;

        public MonthlyCommissionsController(IMonthlyCommissionService commissionService)
        {
            _commissionService = commissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? agencyId = null,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            var result = await _commissionService.GetPaginatedCommissionsAsync(pageNumber, pageSize, agencyId, year, month);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var commission = await _commissionService.GetByIdAsync(id);
                return Ok(commission);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}