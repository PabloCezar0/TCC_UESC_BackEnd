using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Rota.Application.Validation;

namespace Rota.Application.DTOs;

public record PasswordResetConfirmDTO(


    [Required(ErrorMessage = "Token é obrigatório.")]
    string Token,
    [Required(ErrorMessage = "Nova senha é obrigatória.")]
    [MinLength(ValidationRules.MinPasswordLength, ErrorMessage = ValidationRules.MinPasswordLengthError)]
    [RegularExpression(ValidationRules.StrongPasswordRegex, ErrorMessage = ValidationRules.StrongPasswordError)]
    [DataType(DataType.Password)]
    [Display(Name = "Nova Senha")]
    string NewPassword
);