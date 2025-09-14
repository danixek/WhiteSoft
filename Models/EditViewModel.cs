using System.ComponentModel.DataAnnotations;
using WhiteSoft.Handlers;

namespace WhiteSoft.Models
{
    public class EditViewModel : ISecurityModel
    {
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
    }
}
