using System.ComponentModel.DataAnnotations;
using Rota.Application.Validation;

namespace CleanArchMvc.API.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = ValidationRules.EmailError)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(ValidationRules.MinPasswordLength, ErrorMessage = ValidationRules.MinPasswordLengthError)]
        [RegularExpression(ValidationRules.StrongPasswordRegex,
        ErrorMessage = ValidationRules.StrongPasswordError)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
