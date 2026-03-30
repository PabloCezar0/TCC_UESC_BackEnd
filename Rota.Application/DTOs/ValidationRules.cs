namespace Rota.Application.Validation
{
    public static class ValidationRules
    {
        public const string NameRegex = @"^[\p{L}\p{M}\s']+$";
        public const string NameError = "O nome deve conter apenas letras.";

        public const string EmailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        public const string EmailError = "E-mail inválido.";

        public const string StrongPasswordRegex =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?:{}|<>]).{8,}$";
        public const string StrongPasswordError =
            "A senha precisa ter ao menos uma letra maiúscula, uma minúscula, um número.";

        public const int MinPasswordLength = 8;
        public const string MinPasswordLengthError = "A senha deve ter no mínimo 8 caracteres.";
    }
}
