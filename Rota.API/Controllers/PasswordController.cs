using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rota.Application.DTOs;
using Rota.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IPasswordResetService _svc;
    private readonly ILogger<PasswordController> _logger;

    public PasswordController(
        IPasswordResetService svc,
        ILogger<PasswordController> logger)
    {
        _svc    = svc;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("reset-request")]
    public async Task<IActionResult> ResetRequest([FromBody] PasswordResetRequestDTO dto)
    {
        _logger.LogInformation("Password reset-request iniciada para {Email}", dto.Email);
        await _svc.RequestResetAsync(dto.Email);
        _logger.LogInformation("Password reset-request concluída para {Email}", dto.Email);
        return Ok("Se existir, enviaremos as instruções por e-mail.");
    }

    [AllowAnonymous]
    [HttpPost("reset-confirm")]
    public async Task<IActionResult> ResetConfirm([FromBody] PasswordResetConfirmDTO dto)
    {
        _logger.LogInformation("Password reset-confirm iniciada com token {Token}", dto.Token);
        var ok = await _svc.ConfirmResetAsync(dto.Token, dto.NewPassword);
        if (ok)
        {
            _logger.LogInformation("Password reset-confirm bem-sucedida para token {Token}", dto.Token);
            return Ok("Senha redefinida.");
        }
        else
        {
            _logger.LogWarning("Password reset-confirm falhou para token {Token}", dto.Token);
            return BadRequest("Token inválido ou expirado.");
        }
    }
}
