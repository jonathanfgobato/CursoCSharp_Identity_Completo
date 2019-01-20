using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class ValidatorPassword : IIdentityValidator<string>
    {
        public int RequiredLength { get; set; }
        public bool RequireNonLetterOrDigit { get; set; }
        public bool RequiredLowerCase { get; set; }
        public bool RequiredUpperCase { get; set; }
        public bool RequiredDigit{ get; set; }

        public async Task<IdentityResult> ValidateAsync(string item)
        {

            var errors = new List<string>();

            if (!VerifyRequiredSize(item))
                errors.Add($"A senha deve conter no minimo {RequiredLength} caracteres");

            if (RequireNonLetterOrDigit && !VerifySpecialCharacteres(item))
                errors.Add("A senha deve conter caracteres especiais");

            if (RequiredLowerCase && !VerifyLowerCase(item))
                errors.Add("A senha deve conter ao menos uma letra miniscula");

            if (RequiredUpperCase && !VerifyUpperCase(item))
                errors.Add("A senha deve conter ao menos uma letra maiuscula");

            if (RequiredDigit && !VerifyNumbers(item))
                errors.Add("A senha deve conter ao menos um digito");

            if (errors.Any())
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;
        }

        private bool VerifyRequiredSize(string password) =>
            password?.Length >= RequiredLength;

        private bool VerifySpecialCharacteres(string password) =>
            Regex.IsMatch(password, @"^[a-zA-Z0-9 ]*$");

        private bool VerifyLowerCase(string password) =>
            password.Any(char.IsLower);

        private bool VerifyUpperCase(string password) =>
            password.Any(char.IsUpper);

        private bool VerifyNumbers(string password) =>
            password.Any(char.IsDigit);
    }
}