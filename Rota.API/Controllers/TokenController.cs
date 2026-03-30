using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rota.Domain.Common;
using Rota.Domain.Interfaces;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;
using CleanArchMvc.API.Models;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IAuthenticate _auth;
        private readonly IUserService _userService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(
            IAuthenticate auth,
            IUserService userService,
            ILogger<TokenController> logger)
        {
            _auth = auth;
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginModel dto)
        {
            _logger.LogInformation("Login iniciado para {Email}", dto.Email);

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(ModelState);
            }

            
            var user = await _userService.GetUserForLoginAsync(dto.Email);

            
            if (user is null)
            {
                _logger.LogWarning("Login falhou: usuário inexistente {Email}", dto.Email);
                
                return Unauthorized(new ApiResponseErrorDTO { StatusCode = 401, Message = "Usuário ou senha inválidos." });
            }

            
            if (!user.IsActive)
            {
                _logger.LogWarning("Login bloqueado: usuário inativo {Email}", dto.Email);
                return Unauthorized(new ApiResponseErrorDTO { StatusCode = 401, Message = "Usuário inativo. Contate o administrador." });
            }

           
            AuthResult result = await _auth.Authenticate(dto.Email, dto.Password);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login falhou (senha incorreta) para {Email}", dto.Email);
                return Unauthorized(new ApiResponseErrorDTO
                {
                    StatusCode = 401,
                    Message = "Usuário ou senha inválidos." 
                });
            }

            var response = new AuthResponseDTO
            {
                Token = result.Token!,
                Expiration = result.ExpiresAt!.Value,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(), 
                AgencyId = user.AgencyId
            };

            _logger.LogInformation("Login bem-sucedido para {Email}", dto.Email);
            return Ok(response);
        }
    }
}
