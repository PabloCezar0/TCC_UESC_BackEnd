using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rota.Application.DTOs;
using Rota.Application.Services;
using Rota.Domain.Entities;
using Rota.Domain.Validation;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Rota.Domain.Enums;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionInsertService _transactionInsertService;

        private readonly TransactionService _transactionService;

        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(TransactionInsertService transactionInsertService, TransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionInsertService = transactionInsertService;
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("import")]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)}")]
        [SwaggerOperation(
            Summary = "Importa e salva as comissões do ciclo mensal padrão",
            Description = "Dispara a rotina que busca as transações do período (dia 26 do mês anterior até dia 25 do mês atual), as processa e salva os registros de comissão no banco de dados, evitando duplicatas."
        )]
        [SwaggerResponse(200, "Importação concluída.", typeof(object))]
        [SwaggerResponse(500, "Ocorreu um erro interno durante a importação.")]
        public async Task<IActionResult> ImportMonthlyCycle()
        {
            try
            {
                var savedCount = await _transactionInsertService.ImportAndSaveMonthlyCycleAsync();
                return Ok(new { message = $"Operação concluída. {savedCount} novas transações foram importadas para o ciclo padrão." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ocorreu um erro ao importar as transações: {ex.Message}" });
            }
        }

        [HttpPost("import-by-date")]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)}")]
        [SwaggerOperation(
            Summary = "Importa e salva transações para um período específico",
            Description = "Dispara a rotina de importação para um intervalo de datas fornecido (formato: AAAA-MM-DD)."
        )]
        [SwaggerResponse(200, "Importação concluída.", typeof(object))]
        [SwaggerResponse(400, "Parâmetros inválidos. A data de início não pode ser posterior à data de fim.", typeof(object))]
        [SwaggerResponse(500, "Ocorreu um erro interno durante a importação.")]
        public async Task<IActionResult> ImportByDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { error = "A data de início não pode ser posterior à data de fim." });
            }

            try
            {
                var savedCount = await _transactionInsertService.ImportAndSavePeriodAsync(startDate, endDate);
                return Ok(new { message = $"Operação concluída. {savedCount} novas comissões foram importadas para o período de {startDate:dd/MM/yyyy} a {endDate:dd/MM/yyyy}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ocorreu um erro ao importar as comissões: {ex.Message}" });
            }
        }
        

        [HttpGet("transactions")]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)},{nameof(UserRole.Financeiro)}")]
        
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetAll()
        {
            _logger.LogInformation("Get all transactions iniciada.");
            try
            {
                var transactionlist = await _transactionService.GetAllAsync();
                if (!transactionlist.Any())
                {
                    _logger.LogWarning("GetAll : nenhuma transação encontrada");
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Nenhuma transação encontrada"
                    });
                }
                _logger.LogInformation("GetALL: {Count} transações encontradas", transactionlist.Count());
                return Ok(transactionlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll: erro interno do servidor");
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }

        }

        [HttpGet("transactionsPaginated")]
        public async Task<ActionResult> GetPaginatedTransactions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? type = null,
            [FromQuery] int? agencyId = null,
            [FromQuery] decimal? minValue = null,
            [FromQuery] decimal? maxValue = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            _logger.LogInformation("GetPaginatedTransactions iniciada: page= {Page}, size = {Size}", pageNumber, pageSize);
            if (pageNumber < 1)
            {
                _logger.LogWarning("GetPaginatedTransactions: Page Number inválido {Page}", pageNumber);
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "O número da página deve ser maior ou igual a 1."
                });
            }
            try
            {
                var transactionlist = await _transactionService.GetPaginatedTransactionsAsync(pageNumber, pageSize, type, agencyId, minValue, maxValue, startDate, endDate);

                _logger.LogInformation("GetPaginatedTransactions: {Total} registros retornados", transactionlist.TotalItems);

                return Ok(transactionlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPaginatedTransactions: erro interno do servidor");
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }

        }

        [HttpGet("{id:int}")]
       //[Authorize(Roles = $"{nameof(UserRole.Administrador)},{nameof(UserRole.Financeiro)}")]
      
        public async Task<ActionResult<TransactionDTO>> GetById(int id)
        {
            _logger.LogInformation("GetById iniciado para id={Id}", id);

            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);

                if (transaction == null)
                {
                    _logger.LogWarning("GetById: transação id={Id} não encontrada.", id);
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Transação não encontrada."
                    });
                }

                _logger.LogInformation("GetById concluído com sucesso para id={Id}", id);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById: erro interno ao buscar transação id={Id}", id);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }
        }



        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> Post([FromBody] TransactionRegisterDTO dto)
        {
            _logger.LogInformation("POST iniciado para criar nova transação.");

            try
            {
                await _transactionService.AddAsync(dto);
                _logger.LogInformation("POST concluído com sucesso.");

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST: erro ao inserir uma transação.");
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] TransactionRegisterDTO dto)
        {
            _logger.LogInformation("PUT iniciado para atualizar transação id={Id}", id);

            try
            {
                var existing = await _transactionService.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("PUT: transação id={Id} não encontrada.", id);
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Transação não encontrada."
                    });
                }

                await _transactionService.UpdateAsync(id, dto);
                _logger.LogInformation("PUT concluído: transação id={Id} atualizada.", id);

                return Ok(new { message = "Transação atualizada com sucesso.", Transaction = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT: erro ao atualizar transação id={Id}", id);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TransactionDTO>> Delete(int id)
        {
            _logger.LogInformation("DELETE iniciado para id={Id}", id);

            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction != null)
            {
                try
                {
                    await _transactionService.RemoveAsync(id);
                    _logger.LogInformation("DELETE concluído: transação id={Id} removida.", id);

                    return Ok(transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DELETE: erro ao excluir transação id={Id}", id);
                    return StatusCode(500, new ApiResponseErrorDTO
                    {
                        StatusCode = 500,
                        Message = "Erro ao excluir transação.",
                        Detail = ex.Message
                    });
                }
            }

            _logger.LogWarning("DELETE: transação id={Id} não encontrada.", id);
            return NotFound(new ApiResponseErrorDTO
            {
                StatusCode = 404,
                Message = "Transação não encontrada."
            });
        }

        [HttpPost("manual")]
        public async Task<IActionResult> CreateManual([FromBody] TransactionRegisterManualDTO dto)
        {
            try
            {
                var createdTransaction = await _transactionService.CreateManualAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
            }
            catch (DomainExceptionValidation ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



    }
}