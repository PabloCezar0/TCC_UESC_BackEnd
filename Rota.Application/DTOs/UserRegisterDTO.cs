using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Rota.Application.Validation;
using Rota.Domain.Enums;  

namespace Rota.Application.DTOs
{
    public class UserRegisterDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome precisa ter 3 caracteres ou mais.")]
        [RegularExpression(ValidationRules.NameRegex, ErrorMessage = ValidationRules.NameError)]
        [DisplayName("Nome")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = ValidationRules.EmailError)]
        [RegularExpression(ValidationRules.EmailRegex, ErrorMessage = ValidationRules.EmailError)]
        [DisplayName("E-mail")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Agência é obrigatória.")]
        [DisplayName("Agência (ID)")]
        public int? AgencyId { get; set; }

        [Required(ErrorMessage = "Perfil é obrigatório.")]
        [DisplayName("Perfil")]
        public UserRole Role { get; set; }
    }
}
