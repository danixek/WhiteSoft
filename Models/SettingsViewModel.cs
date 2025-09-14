using System.ComponentModel.DataAnnotations;
using WhiteSoft.Handlers;

namespace WhiteSoft.Models
{
    public class SettingsViewModel : ISecurityModel
    {
        [AntiXss]
        public string? Id { get; set; }

        [AntiXss]
        [Required(ErrorMessage = "Jméno je povinné")]
        [Display(Name = "Jméno")]
        public string? FirstName { get; set; }

        [AntiXss]
        [Required(ErrorMessage = "Příjmení je povinné")]
        [Display(Name = "Příjmení")]
        public string? LastName { get; set; }

        [AntiXss]
        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Neplatná emailová adresa.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [AntiXss]
        [Required(ErrorMessage = "Heslo je povinné.")]
        [StringLength(50, ErrorMessage = "Heslo musí mít mezi 6 a 50 znaky.", MinimumLength = 6)]
        [Display(Name = "Heslo")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [AntiXss]
        [Required(ErrorMessage = "Potvrzení hesla je povinné.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hesla se neshodují.")]
        [Display(Name = "Potvrdit heslo")]
        public string? ConfirmPassword { get; set; }

        public bool TOTP { get; set; } = false;
    }
}
