using System.ComponentModel.DataAnnotations;
using Rota.Application.Validation;

namespace Rota.Application.DTOs

{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(ValidationRules.MinPasswordLength, ErrorMessage = ValidationRules.MinPasswordLengthError)]
        [RegularExpression(ValidationRules.StrongPasswordRegex,
        ErrorMessage = ValidationRules.StrongPasswordError)]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(ValidationRules.MinPasswordLength, ErrorMessage = ValidationRules.MinPasswordLengthError)]
        [RegularExpression(ValidationRules.StrongPasswordRegex,
        ErrorMessage = ValidationRules.StrongPasswordError)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

    }
}
