using System.ComponentModel.DataAnnotations;
using WhiteSoft.Handlers;

namespace WhiteSoft.Models
{
    public class LoginViewModel : ISecurityModel
    {
        [AntiXss]
        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Neplatná emailová adresa.")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [AntiXss]
        [Required(ErrorMessage = "Heslo je povinné")]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public required string Password { get; set; }

        [Display(Name = "Zapamatovat tento počítač")]
        public bool RememberMe { get; set; }
    }
}
