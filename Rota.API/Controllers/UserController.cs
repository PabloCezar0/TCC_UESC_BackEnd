using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using Rota.Domain.Enums;
using System.Security.Claims;
using Rota.Domain.Common;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }



        [HttpGet]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)}")]
        [ProducesResponseType(typeof(IEnumerable<UserDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            _logger.LogInformation("GetAll iniciada");
            try
            {
                var users = await _userService.GetUsersAsync();
                if (!users.Any())
                {
                    _logger.LogWarning("GetAll: nenhum usuário encontrado");
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Nenhum usuário encontrado."
                    });
                }
                _logger.LogInformation("GetAll: {Count} usuários retornados", users.Count());
                return Ok(users);
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

        [HttpGet("getAllPaginated")]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)}")]
        [ProducesResponseType(typeof(PaginatedResult<UserDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetPaginatedUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetPaginatedUsers iniciada: page={Page}, size={Size}", pageNumber, pageSize);
            if (pageNumber < 1)
            {
                _logger.LogWarning("GetPaginatedUsers: pageNumber inválido {Page}", pageNumber);
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "O número da página deve ser maior ou igual a 1."
                });
            }
            try
            {
                var page = await _userService.GetPaginatedUsersAsync(pageNumber, pageSize);
                _logger.LogInformation("GetPaginatedUsers: {Total} registros retornados", page.TotalItems);
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPaginatedUsers: erro interno do servidor");
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet("{id:int}", Name = "GetUserById")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDTO>> GetById(int id)
        {
            _logger.LogInformation("GetById iniciada para id={Id}", id);
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("GetById: usuário não encontrado id={Id}", id);
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Usuário não encontrado."
                    });
                }
                _logger.LogInformation("GetById: usuário retornado id={Id}", id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById: erro interno id={Id}", id);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }
        }

        
        [HttpGet("getByEmail")]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)}")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDTO>> GetByEmail([FromQuery] string email)
        {
            _logger.LogInformation("GetByEmail iniciada para email={Email}", email);
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("GetByEmail: email vazio");
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "E-mail é obrigatório."
                });
            }

            try
            {
                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("GetByEmail: usuário não encontrado email={Email}", email);
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Usuário não encontrado."
                    });
                }
                _logger.LogInformation("GetByEmail: usuário retornado email={Email}", email);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByEmail: erro interno email={Email}", email);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro interno do servidor.",
                    Detail = ex.Message
                });
            }
        }


        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        [ProducesResponseType(typeof(UserRegisterDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromBody] UserRegisterDTO dto)
        {
            _logger.LogInformation("Post iniciada para email={Email}", dto.Email);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                _logger.LogWarning("Post: validação falhou para email={Email}", dto.Email);
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "Erro de validação.",
                    Errors = errors
                });
            }
            try
            {
                await _userService.AddAsync(dto);
                _logger.LogInformation("Post: usuário criado email={Email}", dto.Email);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post: erro ao registrar usuário email={Email}", dto.Email);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro ao registrar usuário.",
                    Detail = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        [ProducesResponseType(typeof(UserUpdateDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserUpdateDTO>> Put(int id, [FromBody] UserUpdateDTO dto)
        {
            _logger.LogInformation("Put iniciada id={Id}", id);
            if (id != dto.Id)
            {
                _logger.LogWarning("Put: URL id ({UrlId}) difere do body ({BodyId})", id, dto.Id);
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "O ID da URL não confere com o do corpo."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                _logger.LogWarning("Put: validação falhou id={Id}", id);
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "Erro de validação.",
                    Errors = errors
                });
            }

            try
            {
                await _userService.UpdateAsync(dto);
                _logger.LogInformation("Put: usuário atualizado id={Id}", id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Put: erro ao atualizar usuário id={Id}", id);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro ao atualizar usuário.",
                    Detail = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDTO>> Delete(int id)
        {
            _logger.LogInformation("Delete iniciada id={Id}", id);
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Delete: usuário não encontrado id={Id}", id);
                    return NotFound(new ApiResponseErrorDTO
                    {
                        StatusCode = 404,
                        Message = "Usuário não encontrado."
                    });
                }

                await _userService.RemoveAsync(id);
                user.Password = null;
                _logger.LogInformation("Delete: usuário excluído id={Id}", id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete: erro ao excluir usuário id={Id}", id);
                return StatusCode(500, new ApiResponseErrorDTO
                {
                    StatusCode = 500,
                    Message = "Erro ao excluir usuário.",
                    Detail = ex.Message
                });
            }
        }


        [HttpGet("deleted")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        [ProducesResponseType(typeof(IEnumerable<UserDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetDeleted()
        {
            _logger.LogInformation("GetDeleted iniciada");
            var list = await _userService.GetDeletedAsync();
            _logger.LogInformation("GetDeleted: {Count} registros retornados", list.Count());
            return Ok(list);
        }

        [HttpPut("reactivate")]
        [Authorize(Roles = nameof(UserRole.Administrador))]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDTO>> Reactivate([FromQuery] string email)
        {
            _logger.LogInformation("Reactivate iniciada para email={Email}", email);
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Reactivate: email vazio");
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "E-mail é obrigatório."
                });
            }
            try
            {
                var user = await _userService.ReactivateAsync(email);
                _logger.LogInformation("Reactivate: usuário reativado email={Email}", email);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Reactivate: usuário não encontrado email={Email}", email);
                return NotFound(new ApiResponseErrorDTO
                {
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Reactivate: conflito ao reativar email={Email}", email);
                return Conflict(new ApiResponseErrorDTO
                {
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }


        [HttpPut("active-or-inactive")]
        [Authorize(Roles = $"{nameof(UserRole.Administrador)}")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseErrorDTO), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDTO>> SetActivation([FromQuery] string email, [FromQuery] bool active)
        {
            _logger.LogInformation("SetActivation (email) iniciada email={Email} active={Active}", email, active);

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("SetActivation: email vazio");
                return BadRequest(new ApiResponseErrorDTO
                {
                    StatusCode = 400,
                    Message = "E-mail é obrigatório."
                });
            }

            try
            {
                var dto = await _userService.SetActivationByEmailAsync(email, active);
                _logger.LogInformation("SetActivation concluída: email={Email}, ativo={Active}", email, active);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "SetActivation: não encontrado email={Email}", email);
                return NotFound(new ApiResponseErrorDTO { StatusCode = 404, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "SetActivation: conflito email={Email}", email);
                return Conflict(new ApiResponseErrorDTO { StatusCode = 409, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetActivation: erro interno email={Email}", email);
                return StatusCode(500, new ApiResponseErrorDTO { StatusCode = 500, Message = "Erro interno do servidor.", Detail = ex.Message });
            }
        }


        [HttpPost("change-password")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            _logger.LogInformation("ChangePassword iniciada para usuário logado");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? throw new InvalidOperationException("Token inválido.");
            var userId = int.Parse(userIdClaim);

            await _userService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            _logger.LogInformation("ChangePassword concluída com sucesso userId={UserId}", userId);
            return NoContent();
        }
    }
}
